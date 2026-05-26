import type { ReactNode } from 'react'

interface AuthLayoutProps {
  children: ReactNode
}

const AuthBrand = () => (
  <div
    aria-label="War Of Tanks"
    className="mb-11 flex items-center gap-3 font-mono text-[13px] font-bold tracking-[2.34px] text-[#e7ecef] md:mb-[72px]"
  >
    <span className="relative grid size-[22px] place-items-center border border-[#e7ecef]">
      <span className="size-2.5 border border-[#e7ecef]" />
      <span className="absolute size-[5px] rounded-full bg-[#5ebc7b]" />
    </span>
    <span>WAR OF TANKS</span>
  </div>
)

const AuthHero = () => (
  <section
    aria-label="War Of Tanks tactical briefing"
    className="relative min-h-[220px] overflow-hidden border-b border-[#2a313b] bg-[#0e1116] md:min-h-svh md:border-r md:border-b-0"
  >
    <div className="absolute bottom-5 left-6 z-10 text-left md:bottom-12 md:left-12">
      <p className="text-[22px] leading-[1.15] font-semibold tracking-normal text-[#e7ecef] md:text-[28px]">
        Capture the centre.
      </p>
      <span className="mt-2.5 block text-sm text-[#98a1ad]">
        Hold the zone. Outscore the enemy crew.
      </span>
    </div>
  </section>
)

const AuthLayout = ({ children }: AuthLayoutProps) => {
  return (
    <main className="grid min-h-svh grid-cols-1 overflow-hidden bg-[#0e1116] text-[#e7ecef] md:grid-cols-2">
      <AuthHero />

      <section className="grid min-h-0 place-items-center bg-[#0e1116] px-5 py-9 md:min-h-svh md:p-12">
        <div className="w-full max-w-[380px] text-left">
          <AuthBrand />
          {children}
        </div>
      </section>
    </main>
  )
}

export default AuthLayout
