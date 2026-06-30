const BASE = '/api';

export async function getOpenTasks() {
    const res = await fetch(`${BASE}/tasks`);
    if (!res.ok) throw new Error('Failed to fetch tasks');
    return res.json();
}