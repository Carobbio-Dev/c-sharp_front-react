import { useState } from 'react'
import { FontAwesomeIcon } from '@fortawesome/react-fontawesome'
import { faPaperPlane, faSpinner, faCheckCircle, faExclamationTriangle } from '@fortawesome/free-solid-svg-icons'

function App() {
  const [response, setResponse] = useState<string>('')
  const [loading, setLoading] = useState<boolean>(false)
  const [status, setStatus] = useState<'idle' | 'success' | 'error'>('idle')

  const handlePing = async () => {
    setLoading(true)
    setStatus('idle')
    try {
      // Use relative endpoint if on same host, or fallback to http://localhost:5000/api/ping if on dev server (e.g. port 5173)
      const apiUrl = window.location.port === '5173' ? 'http://localhost:5000/api/ping' : '/api/ping'
      const res = await fetch(apiUrl)
      if (!res.ok) {
        throw new Error(`HTTP error! status: ${res.status}`)
      }
      const data = await res.json()
      setResponse(JSON.stringify(data, null, 2))
      setStatus('success')
    } catch (err: unknown) {
      const errorMessage = err instanceof Error ? err.message : String(err)
      setResponse(`Error connecting to server: ${errorMessage}`)
      setStatus('error')
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="min-h-screen bg-slate-900 text-slate-100 flex flex-col items-center justify-center p-6 font-sans">
      <div className="w-full max-w-xl bg-slate-800 rounded-2xl shadow-xl border border-slate-700 p-8 space-y-6">
        <header className="text-center space-y-2">
          <h1 className="text-3xl font-bold text-indigo-400">
            Fullstack C# & React App
          </h1>
          <p className="text-slate-400 text-sm">
            Powered by ASP.NET Core Web API, React, TypeScript, Vite & Tailwind CSS
          </p>
        </header>

        <div className="flex justify-center">
          <button
            type="button"
            onClick={handlePing}
            disabled={loading}
            className="flex items-center gap-2 bg-indigo-600 hover:bg-indigo-500 disabled:bg-indigo-800 text-white font-semibold py-3 px-6 rounded-xl transition-all shadow-lg hover:shadow-indigo-500/25 active:scale-95 cursor-pointer disabled:cursor-not-allowed"
          >
            {loading ? (
              <FontAwesomeIcon icon={faSpinner} className="animate-spin text-lg" />
            ) : (
              <FontAwesomeIcon icon={faPaperPlane} className="text-lg" />
            )}
            <span>{loading ? 'Sending Ping...' : 'Send Ping to Backend'}</span>
          </button>
        </div>

        <div className="space-y-2">
          <div className="flex items-center justify-between text-sm font-medium text-slate-300">
            <span>Backend Response:</span>
            {status === 'success' && (
              <span className="text-emerald-400 flex items-center gap-1">
                <FontAwesomeIcon icon={faCheckCircle} /> Connected
              </span>
            )}
            {status === 'error' && (
              <span className="text-rose-400 flex items-center gap-1">
                <FontAwesomeIcon icon={faExclamationTriangle} /> Error
              </span>
            )}
          </div>
          <div className="relative">
            <textarea
              readOnly
              value={response}
              placeholder="Click the button above to request response from backend..."
              rows={6}
              className="w-full bg-slate-950 text-emerald-400 font-mono text-sm p-4 rounded-xl border border-slate-700 focus:outline-none focus:ring-2 focus:ring-indigo-500 resize-none shadow-inner"
            />
          </div>
        </div>
      </div>
    </div>
  )
}

export default App
