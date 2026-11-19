import { useEffect, useId, useRef, type PropsWithChildren } from 'react'
import { X } from 'lucide-react'
import { Button } from './Button'

export function Popup({ title, busy, onClose, children }: PropsWithChildren<{
  title: string; busy: boolean; onClose: () => void
}>) {
  const dialog = useRef<HTMLDialogElement>(null)
  const titleId = useId()
  useEffect(() => {
    const element = dialog.current
    element?.showModal()
    return () => element?.close()
  }, [])

  return <dialog ref={dialog} className="popup" aria-labelledby={titleId} onCancel={(event) => {
    event.preventDefault()
    if (!busy) onClose()
  }}>
    <header><h2 id={titleId}>{title}</h2>
      <Button variant="quiet" aria-label="Cerrar" disabled={busy} onClick={onClose}><X size={18} /></Button>
    </header>
    {children}
  </dialog>
}
