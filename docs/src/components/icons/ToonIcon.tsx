import { DarkMode, Gradient, LightMode } from '@/components/Icon'

export function ToonIcon({ id, color }: { id: string; color?: string }) {
  return (
    <>
      <defs>
        <radialGradient id={`${id}-gradient`} gradientUnits="objectBoundingBox" cx="21.019596%" cy="20.549026%" fx="21.019596%" fy="20.549026%" r="97.51015%" gradientTransform="translate(0.21019596,0.20549026),rotate(45.912495),translate(-0.21019596,-0.20549026)">
          <stop offset="0%" stop-color="#FDE68A" />
          <stop offset="0.59137833%" stop-color="#FDE68A" />
          <stop offset="9.405941%" stop-color="#FDE68A" />
          <stop offset="9.9009905%" stop-color="#F97316" />
          <stop offset="36.633663%" stop-color="#F97316" />
          <stop offset="37.128716%" stop-color="#F59E0B" />
          <stop offset="65.84158%" stop-color="#F59E0B" />
          <stop offset="66.33663%" stop-color="#EF4444" />
          <stop offset="100%" stop-color="#B91C1C" />
        </radialGradient>
        <radialGradient id={`${id}-gradient-dark`} gradientUnits="objectBoundingBox" cx="21.019596%" cy="20.549026%" fx="21.019596%" fy="20.549026%" r="97.51015%" gradientTransform="translate(0.21019596,0.20549026),rotate(45.912495),translate(-0.21019596,-0.20549026)">
          <stop offset="0%" stop-color="#FDE68A" />
          <stop offset="0.59137833%" stop-color="#FDE68A" />
          <stop offset="9.405941%" stop-color="#FDE68A" />
          <stop offset="9.9009905%" stop-color="#F97316" />
          <stop offset="36.633663%" stop-color="#F97316" />
          <stop offset="37.128716%" stop-color="#F59E0B" />
          <stop offset="65.84158%" stop-color="#F59E0B" />
          <stop offset="66.33663%" stop-color="#EF4444" />
          <stop offset="100%" stop-color="#B91C1C" />
        </radialGradient>
        <path d="M0 0L512 0L512 512L0 512L0 0Z" id={`${id}_path_1`} />
        <clipPath id={`${id}_clip_1`}>
          <use xlinkHref={`#${id}_path_1`} />
        </clipPath>
      </defs>
      <LightMode className="">
        <g id={`${id}-PBR`} clipPath={`url(#${id}_clip_1)`}>
          <path d="M0 0L512 0L512 512L0 512L0 0Z" id={`${id}-PBR`} fill="none" fillRule="evenodd" stroke="none" />
          <path d="M52 256C52 143.334 143.334 52 256 52C368.666 52 460 143.334 460 256C460 368.666 368.666 460 256 460C143.334 460 52 368.666 52 256Z" id={`${id}-Oval-Copy`} fill={`url(#${id}-gradient)`} fillRule="evenodd" stroke="none" />
          <g id={`${id}-Oval`}>
            <g clipPath={`url(#${id}_clip_2)`}>
              <use fill="none" stroke="#F59E0B" strokeWidth="52" />
            </g>
          </g>
        </g>
      </LightMode>
      <DarkMode className="">
        <g id={`${id}-PBR`} clipPath={`url(#${id}_clip_1)`}>
          <path d="M0 0L512 0L512 512L0 512L0 0Z" id={`${id}-PBR`} fill="none" fillRule="evenodd" stroke="none" />
          <path d="M27 256C27 129.527 129.527 27 256 27C382.473 27 485 129.527 485 256C485 382.473 382.473 485 256 485C129.527 485 27 382.473 27 256Z" id={`${id}-Oval`} fill="#FDE68A" fill-rule="evenodd" stroke="none" />
          <path d="M52 256C52 143.334 143.334 52 256 52C368.666 52 460 143.334 460 256C460 368.666 368.666 460 256 460C143.334 460 52 368.666 52 256Z" id={`${id}-Oval-Copy`} fill={`url(#${id}-gradient-dark)`} fillRule="evenodd" stroke="none" />
        </g>
      </DarkMode>
    </>
  )
}
