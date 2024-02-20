import { Callout } from '@/components/Callout'
import { QuickLink, QuickLinks } from '@/components/QuickLinks'
import { LinkButton } from '@/components/LinkButton';
import Video from '@/components/Video';
import UnityDemo from '@/components/UnityDemo';
import FAQ from '@/components/FAQ';
import AddToVCC from '@/components/AddToVCC';
import ShaderList from '@/components/ShaderList';

const tags = {
  callout: {
    attributes: {
      title: { type: String },
      type: {
        type: String,
        default: 'note',
        matches: ['note', 'warning'],
        errorLevel: 'critical',
      },
    },
    render: Callout,
  },
  figure: {
    selfClosing: true,
    attributes: {
      src: { type: String },
      alt: { type: String },
      caption: { type: String },
    },
    render: ({ src, alt = '', caption }) => (
      <figure>
        {/* eslint-disable-next-line @next/next/no-img-element */}
        <img src={decodeURIComponent(src)} alt={alt} />
        <figcaption>{caption}</figcaption>
      </figure>
    ),
  },
  video: {
    selfClosing: true,
    attributes: {
      url: { type: String },
      title: { type: String },
    },
    render: Video,
  },
  'unity-demo': {
    selfClosing: true,
    attributes: {
      url: { type: String },
      width: { type: Number },
      height: { type: Number },
    },
    render: UnityDemo,
  },
  'quick-links': {
    render: QuickLinks,
  },
  'link-button': {
    render: LinkButton,
    selfClosing: true,
    attributes: {
      href: { type: String },
      variant: { type: String },
      text: { type: String },
    }
  },
  'quick-link': {
    selfClosing: true,
    render: QuickLink,
    attributes: {
      title: { type: String },
      description: { type: String },
      icon: { type: String },
      iconSize: { type: String },
      href: { type: String },
    },
  },
  'faq': {
    selfClosing: true,
    render: FAQ,
  },
  'add-to-vcc': {
    selfClosing: true,
    render: AddToVCC,
  },
  'shader-list': {
    selfClosing: true,
    render: ShaderList,
  }
}

export default tags
