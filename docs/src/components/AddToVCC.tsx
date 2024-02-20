const ADD_TO_VCC_PAGE = '/add-to-vcc';

export default function AddToVCC() {
  return (
    <div
      className="relative cursor-pointer group rounded-full border text-center border-zinc-400 dark:border-white-600"
      onClick={() => window.location.assign(ADD_TO_VCC_PAGE)}
    >
      <div className="absolute -inset-px rounded-full border-2 border-transparent opacity-0 [background:linear-gradient(var(--quick-links-hover-bg,theme(colors.zinc.50)),var(--quick-links-hover-bg,theme(colors.zinc.50)))_padding-box,linear-gradient(to_top,theme(colors.zinc.400),theme(colors.zinc.400),theme(colors.zinc.500))_border-box] group-hover:opacity-100 dark:[--quick-links-hover-bg:theme(colors.zinc.800)]" />
      <button
        type="button"
        className="relative px-4 py-4 text-zinc-600 dark:text-white text-xl"
        onClick={() => window.location.assign(ADD_TO_VCC_PAGE)}
      >
        Add to VCC
      </button>
    </div>
  )
}