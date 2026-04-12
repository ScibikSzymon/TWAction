import './App.css'
import HomePage from './pages/HomePage';
import UserPanel from './pages/UserPanel';
import MainLayout from './layout/MainLayout';
import { ProtectedRoute } from './components/navigation/ProtectedRoute';
import { createBrowserRouter, RouterProvider } from 'react-router-dom'

const router = createBrowserRouter([
  {
    path: "/", 
    element: <MainLayout />,
    children: [
      {
        path: "/", 
        element: <HomePage/>
      },
      {
        element: <ProtectedRoute requiredRole="Admin" />,
        children: [
          {
            path: "/admin/users",
            element: <UserPanel />
          }
        ]
      }
    ], 
  }
]);


const App = () => {
  return (
    <RouterProvider router={router}/>
  )
}

export default App
