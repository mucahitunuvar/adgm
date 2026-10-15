import type { ReactNode } from 'react';

export interface TreeListNode<TData> {
  id: string;
  data: TData;
  children: ReadonlyArray<TreeListNode<TData>>;
}

export interface TreeListProps<TData> {
  nodes: ReadonlyArray<TreeListNode<TData>>;
  renderNode: (data: TData, depth: number) => ReactNode;
  emptyMessage?: string;
}

/** İç içe ağaç (ör. içerik kategorileri). Her düğüm yalnız `renderNode` ile sunulur. */
export function TreeList<TData>({ nodes, renderNode, emptyMessage }: TreeListProps<TData>) {
  if (nodes.length === 0) {
    return emptyMessage ? <p className="tree-list__empty">{emptyMessage}</p> : null;
  }

  return (
    <ul className="tree-list" role="tree">
      {nodes.map((node) => (
        <TreeListItem key={node.id} node={node} renderNode={renderNode} depth={0} />
      ))}
    </ul>
  );
}

function TreeListItem<TData>({
  node,
  renderNode,
  depth,
}: {
  node: TreeListNode<TData>;
  renderNode: (data: TData, depth: number) => ReactNode;
  depth: number;
}) {
  return (
    <li className="tree-list__item" role="treeitem" aria-expanded={node.children.length > 0 ? true : undefined}>
      <div className="tree-list__row" style={{ paddingLeft: `${depth * 1.5}rem` }}>
        {renderNode(node.data, depth)}
      </div>
      {node.children.length > 0 && (
        <ul className="tree-list__children" role="group">
          {node.children.map((child) => (
            <TreeListItem key={child.id} node={child} renderNode={renderNode} depth={depth + 1} />
          ))}
        </ul>
      )}
    </li>
  );
}
