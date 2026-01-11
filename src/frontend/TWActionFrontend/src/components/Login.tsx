const Login = () => {
  const handleGoogleLogin = async () => {
    try {
      window.location.href = 'http://localhost:8000/auth/google'
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