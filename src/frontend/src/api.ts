import axios from 'axios'

const api = axios.create({
  baseURL: 'http://localhost:5111',
  timeout: 5000,
})

export async function getHello(): Promise<string> {
  const res = await api.get('/')
  return res.data
}
