import { Node, mergeAttributes } from '@tiptap/core';

/** Backend whitelist'indeki `figcaption` düğümü - yalnız `FigureNode.ts`'in içinde var olabilir. */
export const Figcaption = Node.create({
  name: 'figcaption',
  content: 'text*',
  parseHTML() {
    return [{ tag: 'figcaption' }];
  },
  renderHTML({ HTMLAttributes }) {
    return ['figcaption', mergeAttributes(HTMLAttributes), 0];
  },
});
