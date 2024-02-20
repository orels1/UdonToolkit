import NextImage from "next/image";

export default function Image({
  src, 
  alt,
  title,
  width,
  height,
} : {
  src: string;
  alt: string;
  title?: string;
  width: number;
  height: number;
}) {
  return (
    <>
      <NextImage src={src} alt={alt} width={width} height={height} quality={95} className="mb-1" />
      {title && <span className="text-sm text-center mt-3 mb-4 text-slate-400 block">{title}</span>}
    </>
  );
}