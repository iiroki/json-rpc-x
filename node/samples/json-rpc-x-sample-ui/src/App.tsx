import { useState } from 'react'
import reactLogo from './assets/react.svg'
import viteLogo from '/vite.svg'
import './App.css'

const ws = new WebSocket('ws://localhost:5245/ws')

ws.addEventListener('open', () => {
  console.log('WebSocket - Open')
})

ws.addEventListener('close', () => console.log('WebSocket - Close'))

ws.addEventListener('message', msg => {
  console.log('WebSocket - Message:', msg.data)
  const data = JSON.parse(msg.data)
  if (data.method === 'init') {
    console.log('!!! here')
    const request = {
      jsonrpc: '2.0',
      method: 'getUsers',
      id: 'test-id'
    }
  
    ws.send(JSON.stringify(request))
    console.log('Request:', request)
  }
})

setInterval(() => {
  if (ws.readyState == ws.OPEN) {
    const request = {
      jsonrpc: '2.0',
      method: 'ping'
    }
  
    // ws.send(JSON.stringify(request))
  }
}, 1000)

setInterval(() => {
  if (ws.readyState == ws.OPEN) {
    const requestId = Date.now()
    const request = {
      jsonrpc: '2.0',
      method: 'getUser',
      id: requestId.toString(),
      params: [requestId % 10]
    }
  
    ws.send(JSON.stringify(request))
  }
}, 5000)

setInterval(() => {
  if (ws.readyState == ws.OPEN) {
    const request = {
      jsonrpc: '2.0',
      method: 'throwException'
    }
  
    ws.send(JSON.stringify(request))
  }
}, 10000)

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
