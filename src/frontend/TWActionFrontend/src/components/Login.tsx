import React, { useEffect, useState } from 'react'

type User = {
  id: string
  email: string
  displayName?: string
  provider: string
  createdAt: string
}

const Login = () => {
  const [user, setUser] = useState<User | null>(null)

  const handleGoogleLogin = async () => {
    try {
      window.location.href = 'http://localhost:8000/auth/google'
    } catch (err) {
      // eslint-disable-next-line no-console
      console.error('Google login error', err)
    }
  }

  useEffect(() => {
    const fetchMe = async () => {
      try {
        const res = await fetch('http://localhost:8000/auth/me', { credentials: 'include' })
        if (res.ok) {
          const data = await res.json()
          setUser(data)
        } else {
          setUser(null)
        }
      } catch (err) {
        console.error('Error fetching /auth/me', err)
      }
    }

    fetchMe()
  }, [])

  const handleLogout = async () => {
    try {
      await fetch('http://localhost:8000/auth/logout', { method: 'POST', credentials: 'include' })
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