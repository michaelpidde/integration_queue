"Should I suggest a similar BFF approach for a future project where they want a portal site viewable by customers and a number of data-as-service modules (probably microservice/micro UIs...) that can call out to third party APIs to get data, and we store API usage for billing purposes? They probably want the portal shell as its own CSR app (maybe) and the modules as their own CSR apps (maybe) embedded into it. But confidential information and auth and all that should be routed through a BFF I imagine."



Yes, BFF still fits — arguably even more strongly here, because you've added two things that make client-side handling of third-party calls actively dangerous: billing-relevant usage data and third-party API credentials. But "micro-frontends + BFF" has a few extra wrinkles worth thinking through before you commit to a shape.

## Why BFF is non-negotiable here (not just "recommended")

With the auth-only case, BFF was about protecting tokens. Here you have an additional, arguably stronger reason: **usage data used for billing cannot be self-reported by the client.** If a module SPA called a third-party API directly, you'd need to either:
- expose the third-party API key to the browser (leaks the key, lets customers exhaust your quota or run up your bill arbitrarily), or
- have the SPA report "I made N calls" back to your billing system (trivially falsifiable — a customer just doesn't report calls, or reports fewer)

So the third-party calls **must** be proxied server-side, and the usage-metering has to happen at that same proxy point — it's the only place you can trust the count. That's not really a BFF-specific argument, it's a "billing requires a server-side chokepoint" argument, but it happens to line up exactly with where your BFF already needs to sit for auth/secret reasons. Good — one architectural boundary does double duty.

## The real question: one BFF, or one per module?

This is the part that's different from your PiddeCorp/work scenario, where there was implicitly one SPA and one BFF. With independently-built micro-frontend modules, you have a real fork:

**Single shared BFF** — one backend service that all modules call, holds the session, proxies all third-party calls, does all usage metering.
- Simple mental model, one place to audit for the billing-integrity concern above
- Becomes a shared bottleneck across module teams — every module team now has a dependency on a team that owns the BFF, which cuts against the "independent micro-frontend" pitch of letting teams ship modules autonomously
- One codebase gets sole knowledge of every third-party integration's contract, so it stops looking "micro" pretty fast

**BFF per module** (true to the original BFF philosophy — one backend per experience/team)
- Each module team owns their own BFF, their own third-party integration, deploys independently
- Better matches "microservice/micro UI" org intent
- But now you need a way for a customer to be authenticated *once* at the shell level and have that identity/session propagate to N independently-owned BFFs without re-authenticating per module, and you need usage data to land in one place for billing regardless of which module's BFF generated it

In practice, most orgs doing this land on a **hybrid: a thin gateway/shell-BFF that terminates the customer's session, plus per-module BFFs behind it that trust the gateway rather than re-running OAuth.** The gateway validates the session cookie, then either forwards a short-lived internal token (signed JWT scoped to that module, or an internal service-to-service credential) to the module BFF, or the module BFFs sit fully behind the gateway and are never directly reachable by the browser at all. Either way, the module BFF gets "this is customer X, authorized for scope Y" without needing to know anything about Entra/your IdP directly.

## Usage/billing as its own cross-cutting concern

Whichever shape you land on, I'd treat metering as a **shared library or shared internal service**, not something each module BFF reimplements — for the same reason you liked the shared contracts NuGet package on PiddeCorp. Every module's proxy call to a third-party API should funnel through common "record this call, against this customer, against this API, at this timestamp" logic, so:
- billing data has one consistent shape regardless of which team wrote the module
- you have one place to reason about idempotency (did a retried call get double-billed?) and about what happens when the third-party call fails after your meter already incremented

## One thing to flag now, before it bites later: cross-origin cookies

If the shell and modules are genuinely separate CSR apps possibly served from different origins/subdomains and embedded (iframes, or Module Federation pulling in remote bundles), your BFF's session cookie strategy needs explicit thought — browsers (Safari ITP, Chrome's third-party cookie changes) increasingly restrict cookies in cross-origin embedded contexts. If shell and modules all live under one parent domain (`portal.company.com`, `module-a.portal.company.com`), first-party cookies with proper `SameSite`/`Domain` scoping work fine. If they're on genuinely different domains, you're looking at token-relay patterns instead of relying on the browser to just carry a cookie — worth nailing down the domain topology early, since it constrains the auth mechanism, not the other way around.

---

Given where you are with PiddeCorp, this might actually be a good excuse to extend your pilot rather than starting fresh — you've already got the BFF-holds-the-token pattern proven out. Want me to sketch this as a diagram (gateway + module BFFs + shared metering) so you've got something concrete to react to, or draft it as an ADR alongside the other two?












"Let's diagram it out. I grasp the ideas, but I'd like a visual so I can discuss it with my manager sometime. They're thinking that this first shell + single module is going to be easy 2 sprints of work or something, but I didn't really grasp the nuances of the ask until our discussion just now. If I could try to reiterate to make sure I've wrapped my head around it, we need to cover these things:
1) A gateway BFF that just handles the auth cycle and then proxying further communications to the module-level BFFs that are not accessible directly from the UI
2) The portal shell project that initiates the auth cycle and loads in the module CSR apps (somehow)
3) A BFF for Module 1 (which is only accessed via the gateway)
4) The CSR app itself for Module 1
5) A billing tracking library (write once, use for all the little BFF microservices in the future)

So let's plan on that model and diagram it out"



[SVG 1]

That's a solid summary — all five pieces are the right ones, and the ordering matches how I'd sequence the work too. Let me get you a diagram to bring to your manager.Here's the architecture you're describing:A few things worth calling out when you walk your manager through this:

**The dashed borders mean something specific.** Module 1 BFF and the "Module 2, 3..." placeholder are both dashed — Module 1's dashed border means "not directly reachable from the browser, only from the gateway," and the ghost box next to it is there to make a point: the pattern is designed to repeat per module without redesign. That's the strongest argument against treating this as a 2-sprint job — sprint 1-2 gets you *one* module's worth of gateway + module BFF + module UI, but the reason you're building the gateway/module split at all is for the modules that come *after* that.

**The two fan-out arrows from the Gateway BFF** are doing the "not a monolith" argument visually — the gateway's job is routing and session handling, not knowing anything about what Module 1's third-party API looks like. That boundary is what keeps a future Module 2 team from needing to touch the gateway codebase at all.

**The billing library sits off Module 1 BFF, not off the gateway** — that's deliberate. Metering has to happen at the point where the third-party call actually happens (module BFF), not at the gateway, since the gateway doesn't know the specifics of what's being called or what it costs. But it's drawn as a separate shared component precisely so it's not reimplemented per module.

One thing the diagram doesn't show, and that you may want to raise verbally: the shell-loads-module-CSR mechanism ("embeds," top of diagram) is its own design decision — iframe vs. Module Federation vs. web components — and that choice affects whether the Gateway BFF's session cookie is even visible to the embedded module in the first place. Worth a follow-up conversation once you're this far, since it's a separate can of worms from the BFF split itself.











"Can you give me some more detail on how the gateway solves problems to ensure we don't need to reauth from each module? I'm sure that will be something I'll need to explain in more detail."


Good instinct to get this nailed down — this is usually the part that trips people up because it's easy to hand-wave "cookies handle it" without explaining what's actually preventing each module from needing its own login. There are really two separate flows: the one-time login, and then every request after that. Let me break them apart. 

[SVG 2] 

That happens exactly once. Everything downstream of it — including calls into Module 1, and any future Module 2, 3, etc. — reuses that same cookie without touching Entra again: 

[SVG 3]

A few mechanics worth having ready when you explain this, because "the cookie just works" isn't quite the full story:

**Step 2 — why the cookie shows up automatically at all.** This only works cleanly if the browser considers the module API calls *same-origin* (or at least same registrable domain with a `SameSite`-friendly setup) as wherever the login happened. The cleanest way to guarantee this: route everything through **one public origin** — e.g. `portal.company.com/api/module1/*`, `portal.company.com/api/module2/*` — where the Gateway BFF is the only thing exposed to the internet and does path-based routing internally to whichever module BFF owns that path. The module CSR app never calls a different domain directly; it calls a relative path on the same origin it was served from. This sidesteps the cross-origin cookie problems I flagged earlier entirely — no `SameSite=None`, no third-party cookie blocking, no CORS preflight complexity. If you instead go with subdomains per module (`module1.portal.company.com`), it still works, but you need `Domain=.portal.company.com` on the cookie and everyone needs to agree on that convention up front. Path-based routing under one origin is the simpler default; I'd only reach for subdomains if a module genuinely needs to be deployed and scaled as a fully separate origin.

**Steps 3–4 — how the module BFF trusts the gateway without hitting Entra.** The module BFF never sees the user's real Entra tokens and never validates anything against Microsoft. Instead, the gateway mints its own short-lived, internally-signed token (a JWT is the natural choice) on every proxied request, scoped narrowly:
- **Audience-restricted** — the token is signed for `module1` specifically. If it somehow leaked or got replayed against Module 2's BFF, Module 2 would reject it because the audience claim doesn't match.
- **Short expiry** — seconds to low minutes, not the lifetime of the user's session. It's not a replacement session token, it's a per-request assertion of "this request really came from the gateway, for this user, right now."
- **Minimal claims** — user/customer identifier, maybe entitlements/roles if the module needs them for authorization decisions, nothing more. The module BFF doesn't need to know anything about Entra ID's token format, claim shape, or issuer — that knowledge stays entirely inside the gateway.

The module BFF validates this with a symmetric or asymmetric key it shares only with the gateway (asymmetric is cleaner — gateway signs with a private key, every module BFF holds only the public key, so a compromised module can't forge tokens for other modules). This is the mechanism that answers "how does the module BFF know I'm really Michael" without it ever talking to Entra.

**One more layer of defense worth mentioning to your manager explicitly:** this internal token isn't the *only* thing stopping a module BFF from being hit directly — it's defense in depth on top of network isolation. The module BFFs shouldn't have a public ingress at all; they should only be reachable from the gateway's network segment (same private network, no external route, no public DNS record). That's actually the stronger guarantee — the token check matters most in case that network boundary is ever misconfigured or someone spins up a new module BFF without properly locking down ingress.

**Two things this same design gets you for free**, worth mentioning since they'll come up eventually even if not in the pilot's first two sprints:
- **Refresh** — the refresh token lives only in the gateway's session store. When the access token to Entra expires, the gateway refreshes it silently server-side; no module BFF, and no CSR app, ever needs to know a refresh happened.
- **Logout** — killing the session is a single operation at the gateway (delete the session store entry, clear the cookie). Module BFFs hold no independent session state to clean up, since they never had one — they only ever trusted a stream of short-lived, per-request tokens.

If it'd help for the discussion with your manager, I could sketch a third diagram specifically on the "one public origin, path-based routing, module BFFs have no public ingress" network topology piece — that's usually the part that makes the "why can't we just call the module BFF directly and skip the gateway" question answer itself.











"Yes, let's make a diagram for the network topology"



[SVG 4]

A few things this makes concrete that are worth stating explicitly to your manager:

**Only one component in the whole system has a public DNS record and public ingress rules: the Gateway BFF.** Everything in the private network zone — Module 1 BFF today, Module 2/3 BFFs later — should be unreachable from the internet even if someone had the URL. That's not a config detail you bolt on later; it's a provisioning decision (which services get a public ingress/load balancer vs. an internal-only service address) that needs to be right from day one, because retrofitting "wait, this was actually supposed to be private" onto something already deployed with a public endpoint is a much bigger lift than provisioning it correctly from the start.

**Both egress arrows are one-directional by design.** Entra ID and the third-party API never initiate anything back into your network — the Gateway BFF and Module 1 BFF only ever reach *out*. That's a small detail but it's the kind of thing a security review will ask about explicitly, so having it already documented saves a round-trip.

**The dashed border on Module 1 BFF and the ghost box both mean "private network member"** — same visual language as the earlier diagram, just now it's about network reachability specifically rather than the auth-token mechanism. Worth pointing out to your manager that this is the same boundary showing up in two different diagrams for two different reasons (auth trust *and* network isolation) — that's a sign the boundary is a real architectural seam, not something you're inventing for the diagram.

If your manager pushes back with "can't we just skip the private-network provisioning for the pilot and expose Module 1 BFF directly to save time," that's probably the moment to point at this diagram specifically — it's a much easier "no, here's why" than trying to explain it verbally after the fact.