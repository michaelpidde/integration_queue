import Router from 'preact-router';
import Home from './pages/Home';
import Tasks from './pages/Tasks';

export function App() {
  return (
      <Router>
        <Home path="/" />
        <Tasks path="/tasks" />
      </Router>
  );
}