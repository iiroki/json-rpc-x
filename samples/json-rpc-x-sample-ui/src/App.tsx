import { useState } from 'react'
import reactLogo from './assets/react.svg'
import viteLogo from '/vite.svg'
import './App.css'

const clientId = crypto.randomUUID()
const ws = new WebSocket('ws://localhost:5245/json-rpc/ws')
console.log('WebSocket - Client ID:', clientId)

ws.addEventListener('open', () => {
  console.log('WebSocket - Open')
})

ws.addEventListener('close', () => console.log('WebSocket - Close'))

ws.addEventListener('message', msg => {
  const data = JSON.parse(msg.data)
  console.log('WebSocket - Received:', data, '->', data.params)
})

setInterval(() => {
  if (ws.readyState == ws.OPEN) {
    const request = {
      jsonrpc: '2.0',
      method: 'notifyOthers',
      params: [`Hello, this method is from ${clientId}!`]
    }

    ws.send(JSON.stringify(request))
    console.log('WebSocket - Sent:', request)
  }
}, 30000)

setInterval(() => {
  if (ws.readyState == ws.OPEN) {
    const request = {
      jsonrpc: '2.0',
      id: crypto.randomUUID(),
      method: 'hello',
      params: [clientId]
    }

    ws.send(JSON.stringify(request))
    console.log('WebSocket - Sent:', request)
  }
}, 10000)

setInterval(() => {
  if (ws.readyState == ws.OPEN) {
    const request = {
      jsonrpc: '2.0',
      method: 'unknown'
    }
  
    ws.send(JSON.stringify(request))
    console.log('WebSocket - Sent:', request)
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
