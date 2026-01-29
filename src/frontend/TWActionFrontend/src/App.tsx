import './App.css'
import HomePage from './pages/HomePage';
import MainLayout from './layout/MainLayout';
import { createBrowserRouter, RouterProvider } from 'react-router-dom'

const router = createBrowserRouter([
  {
    path: "/", 
    element: <MainLayout />,
    children: [
      {
        path: "/", 
        element: <HomePage/>
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
