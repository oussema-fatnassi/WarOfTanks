import type { ReactNode } from 'react'

interface PageContainerProps {
  children: ReactNode
  className?: string
}

/** Full-width page wrapper matching the Figma 1440 / 32px-padding layout. */
const PageContainer = ({ children, className = '' }: PageContainerProps) => (
  <div className={`mx-auto w-full max-w-[1440px] px-4 py-6 sm:px-8 sm:py-8 ${className}`}>
    {children}
  </div>
)

export default PageContainer
