import React from 'react'
import axios from 'axios'

const Login = () => {
  const handleGoogleLogin = async () => {
    try {
      const res = await axios.get('/auth/google')
      const redirectUrl = res.data?.url || res.request?.responseURL
      if (redirectUrl) {
        window.location.href = redirectUrl
      }
    } catch (err) {
      // eslint-disable-next-line no-console
      console.error('Google login error', err)
    }
  }

  return (
    <div>
      <button onClick={handleGoogleLogin}>Sign in with Google</button>
    </div>
  )
}

export default Login