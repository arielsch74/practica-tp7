import { expect, test } from '@playwright/test'

test('crear una tarea la muestra en la lista, y borrarla la saca', async ({ page }) => {
  await page.goto('/')
  const titulo = `e2e ${Date.now()}`                    // único: no choca con otra corrida
  await page.getByLabel('Título de la nueva tarea').fill(titulo)
  await page.getByRole('button', { name: 'Agregar' }).click()
  await expect(page.getByText(titulo)).toBeVisible()    // el dato que ESTE test creó, sin sleeps
  await page.getByRole('button', { name: `Borrar ${titulo}` }).click()
  await expect(page.getByText(titulo)).toHaveCount(0)   // y el borrado, comprobado
})

test('un título demasiado largo muestra el error y no crea nada', async ({ page }) => {
  await page.goto('/')
  const titulo = `e2e ${Date.now()} ${'x'.repeat(100)}` // pasa el máximo (100 en la app de la cátedra)
  await page.getByLabel('Título de la nueva tarea').fill(titulo)
  await page.getByRole('button', { name: 'Agregar' }).click()
  await expect(page.getByRole('alert')).toBeVisible()   // el usuario ve el error…
  await expect(page.getByText(titulo)).toHaveCount(0)   // …y en la lista no apareció nada
})

test('una tarea creada sigue ahí al recargar, y borrada no vuelve', async ({ page }) => {
  await page.goto('/')
  const titulo = `e2e ${Date.now()} persiste`
  await page.getByLabel('Título de la nueva tarea').fill(titulo)
  await page.getByRole('button', { name: 'Agregar' }).click()
  await expect(page.getByText(titulo)).toBeVisible()
  await page.reload()                                   // lo que se ve ahora lo trae la API desde la base
  await expect(page.getByText(titulo)).toBeVisible()
  await page.getByRole('button', { name: `Borrar ${titulo}` }).click()
  await expect(page.getByText(titulo)).toHaveCount(0)   // el borrado se vio en pantalla
  await page.reload()
  await expect(page.getByText(titulo)).toHaveCount(0)   // el borrado también llegó a la base
})
