import { defineConfig } from '@playwright/test'

export default defineConfig({
  testDir: './e2e',
  timeout: 60_000,                 // tope de CADA test: generoso, porque por internet un pedido puede demorarse
  expect: { timeout: 15_000 },     // tope de CADA aserción: no lo hereda del de arriba (el default es 5 s)
  use: {
    baseURL: process.env.E2E_BASE_URL || 'http://localhost:3000',
    trace: 'on-first-retry',         // la traza se graba en el retry de un fallo (los verdes no la generan)
    screenshot: 'only-on-failure',   // screenshot standalone de cada fallo en el reporte
  },
  retries: 1,                      // 1 retry: absorbe una demora suelta; si un test pasa recién ahí, sale «flaky»
  reporter: [['html', { open: 'never' }], ['list']],
})
