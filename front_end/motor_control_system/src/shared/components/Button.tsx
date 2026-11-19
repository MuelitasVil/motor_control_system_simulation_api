import type { ComponentProps } from 'react'

type Props = ComponentProps<'button'> & { variant?: 'primary' | 'secondary' | 'quiet' }

export function Button({ variant = 'secondary', className = '', type = 'button', ...props }: Props) {
  return <button {...props} type={type} className={`button button--${variant} ${className}`} />
}
