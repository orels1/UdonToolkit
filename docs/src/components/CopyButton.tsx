import clsx from "clsx";

export default function CopyButton({
  className,
  content,
}: {
  className?: string;
  content: string;
}) {
  return (
    <button
      type="button"
      title="Copy to clipboard"
      onClick={() => navigator?.clipboard?.writeText(content)}
      className={clsx(className, 'flex items-center justify-center bg-zubc-800 w-[32px] h-[32px] p-1 ring-1 ring-white/10 hover:ring-zinc-400 transition-all group/inner rounded-md')}
    >
       <svg
        aria-hidden="true"
        viewBox="0 0 20 20"
      >
        <g className="dark:block text-white/40 group-hover/inner:text-white/90 group-active/inner:text-zinc-400 transition-colors">
          <path fill="currentColor" d="M7 3.5A1.5 1.5 0 018.5 2h3.879a1.5 1.5 0 011.06.44l3.122 3.12A1.5 1.5 0 0117 6.622V12.5a1.5 1.5 0 01-1.5 1.5h-1v-3.379a3 3 0 00-.879-2.121L10.5 5.379A3 3 0 008.379 4.5H7v-1z" />
          <path fill="currentColor" d="M4.5 6A1.5 1.5 0 003 7.5v9A1.5 1.5 0 004.5 18h7a1.5 1.5 0 001.5-1.5v-5.879a1.5 1.5 0 00-.44-1.06L9.44 6.439A1.5 1.5 0 008.378 6H4.5z" />
        </g>
      </svg>
    </button>
  )
}