import React, { useEffect, useState } from 'react'
import { getHello } from './api'

export default function App() {
  const [msg, setMsg] = useState<string>('Loading...')

  useEffect(() => {
    getHello()
      .then((res) => setMsg(res))
      .catch((err) => setMsg(String(err)))
  }, [])

  return (
    <div className="p-6">
      <h1 className="text-2xl font-bold">Backend says:</h1>
      <pre className="mt-4">{msg}</pre>
    </div>
  )
}
