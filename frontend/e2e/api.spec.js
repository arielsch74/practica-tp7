import { expect, test } from '@playwright/test'

// La api tiene su propia dirección: NO es la del front. Sin barra al final.
const API = process.env.API_BASE_URL || 'http://localhost:8080'

// una ayuda, para no repetir: pide la lista y la devuelve ya convertida
async function listar (request) {
  const r = await request.get(`${API}/api/tareas`)
  expect(r.status()).toBe(200)
  return await r.json()
}

test('el alta guarda en la base de verdad, y el borrado la saca', async ({ request }) => {
  const titulo = `api ${Date.now()}`                       // único: no choca con otra corrida
  const alta = await request.post(`${API}/api/tareas`, { data: { titulo } })
  expect(alta.status()).toBe(201)                          // la api dice «creada»
  const creada = await alta.json()

  const lista = await listar(request)                      // y la BASE la devuelve: no es un doble
  expect(lista.some(t => t.titulo === titulo)).toBe(true)

  const borrado = await request.delete(`${API}/api/tareas/${creada.id}`)
  expect(borrado.status()).toBe(204)                       // «borrada, no hay nada que devolver»

  const despues = await listar(request)                    // y el borrado, comprobado
  expect(despues.some(t => t.titulo === titulo)).toBe(false)
})

test('un título vacío lo rechaza la api, y no crea nada', async ({ request }) => {
  const antes = await listar(request)

  const alta = await request.post(`${API}/api/tareas`, { data: { titulo: '' } })
  expect(alta.status()).toBe(400)                          // el contrato del error

  const despues = await listar(request)
  expect(despues.length).toBe(antes.length)                // y de verdad no se creó nada
})

test('la lista contesta, y es una lista', async ({ request }) => {
  const todas = await listar(request)
  expect(Array.isArray(todas)).toBe(true)
})
