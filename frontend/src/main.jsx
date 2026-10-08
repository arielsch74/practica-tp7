import React from 'react'
import ReactDOM from 'react-dom/client'
import App from './App.jsx'
import './index.css'
import * as Sentry from '@sentry/react'

// Observabilidad (TP9): errores, tiempos de carga y uso de la aplicación van a Sentry.
// Esta dirección (DSN) NO es un secreto: sólo permite ENVIAR datos, y todo front la expone.
Sentry.init({
  dsn: 'https://bcdf99843dd4a714c46dc1abdb35d237@o4512222547673088.ingest.us.sentry.io/4512222550949888',
  environment: window.location.hostname,   // de qué sitio vino: QA, producción o localhost
  integrations: [Sentry.browserTracingIntegration()],
  tracesSampleRate: 1.0,                   // medir todas las visitas: en una app chica, entra en el plan gratuito
})

ReactDOM.createRoot(document.getElementById('root')).render(
  <React.StrictMode>
    <App />
  </React.StrictMode>,
)
