import clsx from "clsx";
import { useState } from "react";
import { MagnifyingGlassIcon } from "@heroicons/react/20/solid";
import { FunnelIcon } from "@heroicons/react/24/outline";
import Link from "next/link";

const SHADERS = [
  { name: 'All', shaders: [
    { name: 'Base Shader', href:'/docs/orl-standard/base-shader', type: 'PBR', fullName: 'orels1/Standard' },
    { name: 'Audio Link', href:'/docs/orl-standard/audio-link', type: 'PBR', fullName: 'orels1/Standard AudioLink' },
    { name: 'Color Randomisation', href:'/docs/orl-standard/color-randomisation', type: 'PBR', fullName: 'orels1/Standard Color Randomisation' },
    { name: 'Cutout', href:'/docs/orl-standard/base-shader', type: 'PBR', fullName: 'orels1/Standard Cutout' },
    { name: 'Dissolve', href:'/docs/orl-standard/dissolve', type: 'PBR', fullName: 'orels1/Standard Dissolve' },
    { name: 'Dither Fade', href:'/docs/orl-standard/dither-fade', type: 'PBR', fullName: 'orels1/Standard Dither Fade' },
    { name: 'Glass', href:'/docs/orl-standard/glass', type: 'PBR', fullName: 'orels1/Standard Glass' },
    { name: 'Layered Material', href:'/docs/orl-standard/layered-material', type: 'PBR', fullName: 'orels1/Standard Layered Material' },
    { name: 'Layered Parallax', href:'/docs/orl-standard/layered-parallax', type: 'PBR', fullName: 'orels1/Standard Layered Parallax' },
    { name: 'LTCGI', href:'/docs/orl-standard/ltcgi', type: 'PBR', fullName: 'orels1/Standard LTCGI' },
    { name: 'Neon Light', href:'/docs/orl-standard/neon-light', type: 'PBR', fullName: 'orels1/Standard Neon Light' },
    { name: 'Puddles', href:'/docs/orl-standard/puddles', type: 'PBR', fullName: 'orels1/Standard Puddles' },
    { name: 'Pulse', href:'/docs/orl-standard/pulse', type: 'PBR', fullName: 'orels1/Standard Pulse' },
    { name: 'Tessellated Displacement', href:'/docs/orl-standard/tessellated-displacement', type: 'PBR', fullName: 'orels1/Standard Tessellated Displacement' },
    { name: 'Triplanar Effects', href:'/docs/orl-standard/triplanar-effects', type: 'PBR', fullName: 'orels1/Standard Triplanar Effects' },
    { name: 'Vertex Animation', href:'/docs/orl-standard/vertex-animation', type: 'PBR', fullName: 'orels1/Standard Vertex Animation' },
    { name: 'Vertical Fog', href:'/docs/orl-standard/vertical-fog', type: 'PBR', fullName: 'orels1/Standard Vertical Fog' },
    { name: 'Video Screen', href:'/docs/orl-standard/video-screen', type: 'PBR', fullName: 'orels1/Standard Video Screen' },
    { name: 'Toon', href:'/docs/toon/base-shader', type: 'Toon', fullName: 'orels1/Toon/Main' },
    { name: 'Toon Transparent', href:'/docs/toon/base-shader', type: 'Toon', fullName: 'orels1/Toon/Transparent' },
    { name: 'Toon Transparent PrePass', href:'/docs/toon/base-shader', type: 'Toon', fullName: 'orels1/Toon/Transparent PrePass' },
    { name: 'Toon Cutout', href:'/docs/toon/base-shader', type: 'Toon', fullName: 'orels1/Toon/Cutout' },
    { name: 'Toon UV Discard', href:'/docs/toon/uv-discard', type: 'Toon', fullName: 'orels1/Toon/UV Discard' },
    { name: 'Clouds', href:'/docs/vfx/clouds', type: 'VFX', fullName: 'orels1/VFX/Clouds' },
    { name: 'Dither Fade', href:'/docs/orl-standard/dither-fade', type: 'VFX', fullName: 'orels1/VFX/Dither Fade' },
    { name: 'Ghost Lines', href:'/docs/vfx/ghost-lines', type: 'VFX', fullName: 'orels1/VFX/Ghost Lines' },
    { name: 'Glitch Screen', href: '/docs/vfx/glitch-screen', type: 'VFX', fullName: 'orels1/VFX/Glitch Screen' },
    { name: 'Shield', href:'/docs/vfx/shield', type: 'VFX', fullName: 'orels1/VFX/Shield' },
    { name: 'Laser', href:'/docs/vfx/laser', type: 'VFX', fullName: 'orels1/VFX/Laser' },
    { name: 'Layered Parallax', href:'/docs/orl-standard/layered-parallax', type: 'VFX', fullName: 'orels1/VFX/Layered Parallax' },
    { name: 'Holographic Parallax', href:'/docs/vfx/holographic-parallax', type: 'VFX', fullName: 'orels1/VFX/Holographic Parallax' },
    { name: 'Cubemap Screen', href:'/docs/vfx/cubemap-screen', type: 'VFX', fullName: 'orels1/VFX/Cubemap Screen' },
    { name: 'Block Fader', href:'/docs/vfx/block-fader', type: 'VFX', fullName: 'orels1/VFX/Block Fader' },
    { name: 'Unlit Video Screen', href:'/docs/orl-standard/video-screen', type: 'VFX', fullName: 'orels1/VFX/Unlit Video Screen' },
    { name: 'UI Base Shader', href:'/docs/ui/base-shader', type: 'UI', fullName: 'orels1/UI/Main' },
    { name: 'UI Overlay', href:'/docs/ui/base-shader', type: 'UI', fullName: 'orels1/UI/Main Overlay' },
    { name: 'UI Audio Link', href:'/docs/ui/audio-link', type: 'UI', fullName: 'orels1/UI/AudioLink' },
    { name: 'UI Audio Link Overlay', href:'/docs/ui/audio-link', type: 'UI', fullName: 'orels1/UI/AudioLink Overlay' },
    { name: 'Layered Parallax UI', href:'/docs/ui/layered-parallax', type: 'UI', fullName: 'orels1/UI/Layered Parallax' },
    { name: 'Layered Parallax UI Overlay', href:'/docs/ui/layered-parallax', type: 'UI', fullName: 'orels1/UI/Layered Parallax Overlay' },
    { name: 'Scrolling Texture UI', href:'/docs/ui/scrolling-texture', type: 'UI', fullName: 'orels1/UI/Scrolling Texture' },
    { name: 'Scrolling Texture UI Overlay', href:'/docs/ui/scrolling-texture', type: 'UI', fullName: 'orels1/UI/Scrolling Texture Overlay' },
    { name: 'UI Sheen', href:'/docs/ui/sheen', type: 'UI', fullName: 'orels1/UI/Sheen' },
    { name: 'UI Sheen Overlay', href:'/docs/ui/sheen', type: 'UI', fullName: 'orels1/UI/Sheen Overlay' },
    { name: 'UI Video Screen', href:'/docs/orl-standard/video-screen', type: 'PBR', fullName: 'orels1/UI/Video Screen' },
    { name: 'UI Video Screen Overlay', href:'/docs/orl-standard/video-screen', type: 'PBR', fullName: 'orels1/UI/Video Screen Overlay' },
  ] },
  { name: 'PBR', shaders: [
    { name: 'Base Shader', href:'/docs/orl-standard/base-shader', type: 'PBR', fullName: 'orels1/Standard' },
    { name: 'Cutout', href:'/docs/orl-standard/base-shader', type: 'PBR', fullName: 'orels1/Standard Cutout' },
    { name: 'Audio Link', href:'/docs/orl-standard/audio-link', type: 'PBR', fullName: 'orels1/Standard AudioLink' },
    { name: 'Color Randomisation', href:'/docs/orl-standard/color-randomisation', type: 'PBR', fullName: 'orels1/Standard Color Randomisation' },
    { name: 'Dissolve', href:'/docs/orl-standard/dissolve', type: 'PBR', fullName: 'orels1/Standard Dissolve' },
    { name: 'Dither Fade', href:'/docs/orl-standard/dither-fade', type: 'PBR', fullName: 'orels1/Standard Dither Fade' },
    { name: 'Glass', href:'/docs/orl-standard/glass', type: 'PBR', fullName: 'orels1/Standard Glass' },
    { name: 'Layered Material', href:'/docs/orl-standard/layered-material', type: 'PBR', fullName: 'orels1/Standard Layered Material' },
    { name: 'Layered Parallax', href:'/docs/orl-standard/layered-parallax', type: 'PBR', fullName: 'orels1/Standard Layered Parallax' },
    { name: 'LTCGI', href:'/docs/orl-standard/ltcgi', type: 'PBR', fullName: 'orels1/Standard LTCGI' },
    { name: 'Neon Light', href:'/docs/orl-standard/neon-light', type: 'PBR', fullName: 'orels1/Standard Neon Light' },
    { name: 'Puddles', href:'/docs/orl-standard/puddles', type: 'PBR', fullName: 'orels1/Standard Puddles' },
    { name: 'Pulse', href:'/docs/orl-standard/pulse', type: 'PBR', fullName: 'orels1/Standard Pulse' },
    { name: 'Tessellated Displacement', href:'/docs/orl-standard/tessellated-displacement', type: 'PBR', fullName: 'orels1/Standard Tessellated Displacement' },
    { name: 'Triplanar Effects', href:'/docs/orl-standard/triplanar-effects', type: 'PBR', fullName: 'orels1/Standard Triplanar Effects' },
    { name: 'Vertex Animation', href:'/docs/orl-standard/vertex-animation', type: 'PBR', fullName: 'orels1/Standard Vertex Animation' },
    { name: 'Vertical Fog', href:'/docs/orl-standard/vertical-fog', type: 'PBR', fullName: 'orels1/Standard Vertical Fog' },
    { name: 'Video Screen', href:'/docs/orl-standard/video-screen', type: 'PBR', fullName: 'orels1/Standard Video Screen' },
  ] },
  { name: 'Toon', shaders: [
    { name: 'Toon', href:'/docs/toon/base-shader', type: 'Toon', fullName: 'orels1/Toon/Main' },
    { name: 'Toon Transparent', href:'/docs/toon/base-shader', type: 'Toon', fullName: 'orels1/Toon/Transparent' },
    { name: 'Toon Transparent PrePass', href:'/docs/toon/base-shader', type: 'Toon', fullName: 'orels1/Toon/Transparent PrePass' },
    { name: 'Toon Cutout', href:'/docs/toon/base-shader', type: 'Toon', fullName: 'orels1/Toon/Cutout' },
    { name: 'Toon UV Discard', href:'/docs/toon/uv-discard', type: 'Toon', fullName: 'orels1/Toon/UV Discard' },
  ] },
  { name: 'VFX', shaders: [
    { name: 'Clouds', href:'/docs/vfx/clouds', type: 'VFX', fullName: 'orels1/VFX/Clouds' },
    { name: 'Dither Fade', href:'/docs/orl-standard/dither-fade', type: 'PBR', fullName: 'orels1/VFX/Dither Fade' },
    { name: 'Ghost Lines', href:'/docs/vfx/ghost-lines', type: 'VFX', fullName: 'orels1/VFX/Ghost Lines' },
    { name: 'Glitch Screen', href: '/docs/vfx/glitch-screen', type: 'VFX', fullName: 'orels1/VFX/Glitch Screen' },
    { name: 'Patterns', href:'/docs/vfx/patterns', type: 'VFX', fullName: 'orels1/VFX/Patterns'},
    { name: 'Shield', href:'/docs/vfx/shield', type: 'VFX', fullName: 'orels1/VFX/Shield' },
    { name: 'Laser', href:'/docs/vfx/laser', type: 'VFX', fullName: 'orels1/VFX/Laser' },
    { name: 'Layered Parallax', href:'/docs/orl-standard/layered-parallax', type: 'VFX', fullName: 'orels1/VFX/Layered Parallax' },
    { name: 'Holographic Parallax', href:'/docs/vfx/holographic-parallax', type: 'VFX', fullName: 'orels1/VFX/Holographic Parallax' },
    { name: 'Cubemap Screen', href:'/docs/vfx/cubemap-screen', type: 'VFX', fullName: 'orels1/VFX/Cubemap Screen' },
    { name: 'Block Fader', href:'/docs/vfx/block-fader', type: 'VFX', fullName: 'orels1/VFX/Block Fader' },
    { name: 'Unlit Video Screen', href:'/docs/orl-standard/video-screen', type: 'VFX', fullName: 'orels1/VFX/Unlit Video Screen' },
  ] },
  { name: 'UI', shaders: [
    { name: 'UI Base Shader', href:'/docs/ui/base-shader', type: 'UI', fullName: 'orels1/UI/Main' },
    { name: 'UI Overlay', href:'/docs/ui/base-shader', type: 'UI', fullName: 'orels1/UI/Main Overlay' },
    { name: 'UI Audio Link', href:'/docs/ui/audio-link', type: 'UI', fullName: 'orels1/UI/AudioLink' },
    { name: 'UI Audio Link Overlay', href:'/docs/ui/audio-link', type: 'UI', fullName: 'orels1/UI/AudioLink Overlay' },
    { name: 'Layered Parallax UI', href:'/docs/ui/layered-parallax', type: 'UI', fullName: 'orels1/UI/Layered Parallax' },
    { name: 'Layered Parallax UI Overlay', href:'/docs/ui/layered-parallax', type: 'UI', fullName: 'orels1/UI/Layered Parallax Overlay' },
    { name: 'Scrolling Texture UI', href:'/docs/ui/scrolling-texture', type: 'UI', fullName: 'orels1/UI/Scrolling Texture' },
    { name: 'Scrolling Texture UI Overlay', href:'/docs/ui/scrolling-texture', type: 'UI', fullName: 'orels1/UI/Scrolling Texture Overlay' },
    { name: 'UI Sheen', href:'/docs/ui/sheen', type: 'UI', fullName: 'orels1/UI/Sheen' },
    { name: 'UI Sheen Overlay', href:'/docs/ui/sheen', type: 'UI', fullName: 'orels1/UI/Sheen Overlay' },
    { name: 'UI Video Screen', href:'/docs/orl-standard/video-screen', type: 'PBR', fullName: 'orels1/UI/Video Screen' },
    { name: 'UI Video Screen Overlay', href:'/docs/orl-standard/video-screen', type: 'PBR', fullName: 'orels1/UI/Video Screen Overlay' },
  ] },
]

export default function ShaderList() {
  const [selected, setSelected] = useState(0);
  const [filter, setFilter] = useState('');

  return (
    <div className="grid grid-cols-[200px_1fr] gap-2">
      <div className="flex flex-col">
        <div className="flex items-center">
          <FunnelIcon className="h-5 w-5 mr-2 text-gray-400" aria-hidden="true" />
          <p className="my-2">Filters</p>
        </div>
        <div className="space-y-1 mt-[9px] flex flex-col" aria-label="Sidebar">
          {SHADERS.map((item, index) => (
            <button
              key={item.name}
              type="button"
              className={clsx(
                index === selected ? 'ring-amber-400 bg-white/5' : 'ring-transparent dark:hover:ring-slate-600 hover:ring-slate-300',
                'group flex items-center rounded-md px-3 py-2 text-sm font-medium text-slate-800 dark:text-slate-400 ring-1'
              )}
              aria-current={index === selected ? 'page' : undefined}
              onClick={() => setSelected(index)}
            >
              <span className="truncate">{item.name}</span>
              {item.shaders.length ? (
                <span
                  className={clsx(
                    'dark:bg-slate-500 bg-slate-200 text-gray-600 dark:text-gray-200',
                    'ml-auto inline-block rounded-full px-3 text-xs leading-[10px] py-[4px]'
                  )}
                >
                  {item.shaders.length}
                </span>
              ) : null}
            </button>
          ))}
        </div>
      </div>
      <div className="flex flex-col">
        <div className="relative rounded-md">
          <div className="pointer-events-none absolute inset-y-0 left-0 flex items-center pl-3">
            <MagnifyingGlassIcon className="h-5 w-5 text-gray-400" aria-hidden="true" />
          </div>
          <input
            type="text"
            name="shaderSearch"
            id="shaderSearch"
            className="block w-full rounded-md border-0 py-2.5 px-14 bg-transparent dark:bg-slate-800 text-gray-900 dark:text-slate-100 ring-1 ring-inset ring-slate-300 dark:ring-slate-700 placeholder:text-gray-400 hover:ring-slate-500 dark:hover:ring-slate-500 focus:ring-2 focus:ring-inset focus:ring-amber-600 sm:text-sm sm:leading-6 outline-none"
            placeholder="Search shaders"
            value={filter}
            onChange={({ target: { value = '' }}) => setFilter(value)}
          />
        </div>
        <div className="grid grid-cols-1 gap-2 mt-2 sm:grid-cols-2">
          {SHADERS[selected].shaders.filter(sh => sh.name.toLowerCase().includes(filter.toLowerCase())).map((shader) => (
            <div
              key={shader.name}
              className="relative flex px-4 pt-2 items-center space-x-3 rounded-md border border-gray-300 hover:dark:border-gray-600 dark:border-gray-700 bg-white/5"
            >
              {/* <div className="flex-shrink-0">
                <img className="h-10 w-10 rounded-full" src={person.imageUrl} alt="" />
              </div> */}
              <div className="min-w-0 flex-1">
                <Link href={shader.href} className="focus:outline-none">
                  <span className="absolute inset-0" aria-hidden="true" />
                  <p className="text-sm my-0 font-medium text-gray-900 dark:text-slate-200">{shader.name}</p>
                  <p className="truncate my-1 text-xs font-mono text-gray-500 dark:text-slate-500">{shader.fullName}</p>
                  <p className="truncate my-1 text-sm text-gray-500 dark:text-slate-500">{shader.type}</p>
                </Link>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  )
}