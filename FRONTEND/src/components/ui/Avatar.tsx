type Size = 'sm' | 'md' | 'lg'

interface AvatarProps {
  name: string
  size?: Size
  className?: string
}

const sizes: Record<Size, string> = {
  sm: 'size-7 text-[11px]',
  md: 'size-8 text-[12px]',
  lg: 'size-9 text-[13px]',
}

const initials = (name: string) =>
  (name.trim().slice(0, 2) || '??').toUpperCase()

/** Circular initials avatar. */
const Avatar = ({ name, size = 'md', className = '' }: AvatarProps) => (
  <span
    className={`inline-flex shrink-0 items-center justify-center rounded-full border border-line-strong bg-raised font-mono font-bold text-fg ${sizes[size]} ${className}`}
  >
    {initials(name)}
  </span>
)

export default Avatar
