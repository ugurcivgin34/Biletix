export interface Toast {
  id: string
  title?: string
  description?: string
}

export function toast(input: Omit<Toast, "id">) {
  return {
    id: crypto.randomUUID(),
    ...input,
  }
}

