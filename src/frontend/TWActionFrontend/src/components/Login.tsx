import React, { useEffect, useState } from 'react'
import { apiClient } from '../config/api'

type User = {
  id: string
  email: string
  displayName?: string
  provider: string
  createdAt: string
}

const Login = () => {
  const [user, setUser] = useState<User | null>(null)

  const handleGoogleLogin = () => {
    window.location.href = `${apiClient.defaults.baseURL}/auth/google`
  }

  useEffect(() => {
    const fetchMe = async () => {
      try {
        const { data } = await apiClient.get<User>('/auth/me')
        setUser(data)
      } catch (err) {
        console.error('Error fetching /auth/me', err)
        setUser(null)
      }
    }

    fetchMe()
  }, [])

  const handleLogout = async () => {
    try {
      await apiClient.post('/auth/logout')
      setUser(null)
    } catch (err) {
      // eslint-disable-next-line no-console
      console.error('Logout error', err)
    }
  }

  return (
    <div>
      {user ? (
        <div>
          <span>Signed in as {user.email}</span>
          <button onClick={handleLogout}>Logout</button>
        </div>
      ) : (
        <button onClick={handleGoogleLogin}>Sign in with Google</button>
      )}
    </div>
  )
}

export default Login