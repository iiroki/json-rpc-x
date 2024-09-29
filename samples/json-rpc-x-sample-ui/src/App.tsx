import { useState } from 'react'
import reactLogo from './assets/react.svg'
import viteLogo from '/vite.svg'
import './App.css'

const id = crypto.randomUUID()
const ws = new WebSocket('ws://localhost:5245/json-rpc/ws')

ws.addEventListener('open', () => {
  console.log('WebSocket - Open')
})

ws.addEventListener('close', () => console.log('WebSocket - Close'))

ws.addEventListener('message', msg => {
  console.log('WebSocket - Received:', msg.data)
  const data = JSON.parse(msg.data)

  if (data.method === 'server2client' && data.id) {
    const response = {
      jsonrpc: '2.0',
      id: data.id,
      result: { responseFromClient: true }
    }

    ws.send(JSON.stringify(response))
    console.log('WebSocket - Response:', response)
  }

  if (data.method === 'init') {
    const request = {
      jsonrpc: '2.0',
      method: 'getUsers',
      id: crypto.randomUUID()
    }
  
    ws.send(JSON.stringify(request))
  }
})

setInterval(() => {
  if (ws.readyState == ws.OPEN) {
    const request = {
      jsonrpc: '2.0',
      method: 'notifyOthers',
      params: ['client2client', `Hello from: ${id}`]
    }

    ws.send(JSON.stringify(request))
  }
}, 10000)

setInterval(() => {
  if (ws.readyState == ws.OPEN) {
    const request = {
      jsonrpc: '2.0',
      method: 'middleware',
      id: crypto.randomUUID()
    }
  
    ws.send(JSON.stringify(request))
  }
}, 30000)

setInterval(() => {
  if (ws.readyState == ws.OPEN) {
    const request = {
      jsonrpc: '2.0',
      method: 'unknown'
    }
  
    ws.send(JSON.stringify(request))
  }
}, 60000)

const App = () => {
  const [count, setCount] = useState(0)

  return (
    <>
      <div>
        <a href='https://vitejs.dev' target='_blank'>
          <img src={viteLogo} className='logo' alt='Vite logo' />
        </a>
        <a href='https://react.dev' target='_blank'>
          <img src={reactLogo} className='logo react' alt='React logo' />
        </a>
      </div>
      <h1>Vite + React</h1>
      <div className='card'>
        <button onClick={() => setCount((count) => count + 1)}>
          count is {count}
        </button>
        <p>
          Edit <code>src/App.tsx</code> and save to test HMR
        </p>
      </div>
      <p className='read-the-docs'>
        Click on the Vite and React logos to learn more
      </p>
    </>
  )
}

export default App
