using System;
using System.Collections;
using System.Windows.Forms;
using System.Drawing;

namespace Essence_graphics
{
    public class MultiSelectTreeView : TreeView
    {
        protected ArrayList SelNodes;
        protected TreeNode m_firstNode, m_lastNode;

        public MultiSelectTreeView()
        {
            SelNodes = new ArrayList();
        }

        public ArrayList SelectedNodes
        {
            get
            {
                return SelNodes;
            }
            set
            {
                if (SelNodes == null) return;
                removePaintFromNodes();
                SelNodes.Clear();
                SelNodes = value;
                paintSelectedNodes();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            this.DoubleBuffered = true;
            base.OnPaint(e);
        }

        protected override void OnBeforeSelect(TreeViewCancelEventArgs e)
        {
            base.OnBeforeSelect(e);

            bool bControl = (ModifierKeys == Keys.Control);
            bool bShift = (ModifierKeys == Keys.Shift);

            // selecting twice the node while pressing CTRL ?
            if (bControl && SelNodes.Contains(e.Node))
            {
                e.Cancel = true;

                // update nodes
                removePaintFromNodes();
                SelNodes.Remove(e.Node);
                paintSelectedNodes();
                return;
            }

            m_lastNode = e.Node;
            if (!bShift) m_firstNode = e.Node; // store begin of shift sequence
        }

        protected override void OnAfterSelect(TreeViewEventArgs e)
        {
            base.OnAfterSelect(e);

            bool bControl = (ModifierKeys == Keys.Control);
            bool bShift = (ModifierKeys == Keys.Shift);

            if (bControl)
            {
                if (!SelNodes.Contains(e.Node)) // new node ?
                {
                    SelNodes.Add(e.Node);
                }
                else  // not new, remove it from the collection
                {
                    removePaintFromNodes();
                    SelNodes.Remove(e.Node);
                }
                paintSelectedNodes();
            }
            else
            {
                if (bShift)
                {
                    Queue myQueue = new Queue();

                    TreeNode uppernode = m_firstNode;
                    TreeNode bottomnode = e.Node;

                    // case 1 : begin and end nodes are parent
                    bool bParent = isParent(m_firstNode, e.Node); // is m_firstNode parent (direct or not) of e.Node
                    if (!bParent)
                    {
                        bParent = isParent(bottomnode, uppernode);
                        if (bParent) // swap nodes
                        {
                            TreeNode t = uppernode;
                            uppernode = bottomnode;
                            bottomnode = t;
                        }
                    }
                    if (bParent)
                    {
                        TreeNode n = bottomnode;
                        while (n != uppernode.Parent)
                        {
                            if (!SelNodes.Contains(n)) // new node ?
                                myQueue.Enqueue(n);

                            n = n.Parent;
                        }
                    }
                    // case 2 : nor the begin nor the end node are descendant one another
                    else
                    {
                        if ((uppernode.Parent == null && bottomnode.Parent == null) || (uppernode.Parent != null && uppernode.Parent.Nodes.Contains(bottomnode))) // are they siblings ?
                        {
                            int nIndexUpper = uppernode.Index;
                            int nIndexBottom = bottomnode.Index;
                            if (nIndexBottom < nIndexUpper) // reversed?
                            {
                                TreeNode t = uppernode;
                                uppernode = bottomnode;
                                bottomnode = t;
                                nIndexUpper = uppernode.Index;
                                nIndexBottom = bottomnode.Index;
                            }

                            TreeNode n = uppernode;
                            while (nIndexUpper <= nIndexBottom)
                            {
                                if (!SelNodes.Contains(n)) // new node ?
                                    myQueue.Enqueue(n);

                                n = n.NextNode;

                                nIndexUpper++;
                            } // end while

                        }
                        else
                        {
                            if (!SelNodes.Contains(uppernode)) myQueue.Enqueue(uppernode);
                            if (!SelNodes.Contains(bottomnode)) myQueue.Enqueue(bottomnode);
                        }

                    }

                    SelNodes.AddRange(myQueue);

                    paintSelectedNodes();
                    m_firstNode = e.Node; // let us chain several SHIFTs if we like it

                } // end if m_bShift
                else
                {
                    // in the case of a simple click, just add this item
                    if (SelNodes != null && SelNodes.Count > 0)
                    {
                        removePaintFromNodes();
                        SelNodes.Clear();
                    }
                    if (SelNodes == null) SelNodes = new ArrayList();
                    SelNodes.Add(e.Node);
                }
            }
        }

        protected void paintSelectedNodes()
        {
            if (SelNodes == null) return;
            this.BeginUpdate();
            foreach (TreeNode n in SelNodes)
            {
                n.BackColor = SystemColors.Highlight;
                n.ForeColor = SystemColors.HighlightText;
            }
            this.EndUpdate();
        }

        protected void removePaintFromNodes()
        {
            if (SelNodes == null || SelNodes.Count == 0) return;

            this.BeginUpdate();
            
            foreach (TreeNode n in SelNodes)
            {
                n.BackColor = this.BackColor;
                n.ForeColor = this.ForeColor;
            }

            this.EndUpdate();
        }

        protected bool isParent(TreeNode parentNode, TreeNode childNode)
        {
            if (parentNode == childNode)
                return true;

            TreeNode n = childNode;
            bool bFound = false;
            while (!bFound && n != null)
            {
                n = n.Parent;
                bFound = (n == parentNode);
            }
            return bFound;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ResumeLayout(false);
        }

        public void DelNodes()
        {
            if (this.SelectedNodes.Count == 0) return;

            this.BeginUpdate();

            TreeNodeCollection appendTo = ((this.SelectedNodes[0] as TreeNode).Parent == null) ? this.Nodes : (this.SelectedNodes[0] as TreeNode).Parent.Nodes;

            foreach (TreeNode tn in appendTo)
                if (this.SelectedNodes.Contains(tn))
                    tn.Tag = "DELME";

            this.SelectedNodes = null;

            int i = 0;
            int count = appendTo.Count;
            while (i < count)
            {
                TreeNode tn = appendTo[i];
                if (tn.Tag == "DELME")
                {
                    tn.Remove();
                    count--;
                }
                else
                    i++;
            }

            this.EndUpdate();
        }

        public void GroupNodes()
        {
            if (this.SelectedNodes.Count == 0) return;
            this.BeginUpdate();
            TreeNode groupNode = new TreeNode();
            groupNode.Text = "Group";
            groupNode.Name = "Group";
            groupNode.Tag = "Group";
            groupNode.Checked = true;

            TreeNodeCollection appendTo = ((this.SelectedNodes[0] as TreeNode).Parent == null) ? this.Nodes : (this.SelectedNodes[0] as TreeNode).Parent.Nodes;
            int pos = (this.SelectedNodes[0] as TreeNode).Index;

            foreach (TreeNode tn in appendTo)
                if (this.SelectedNodes.Contains(tn))
                {
                    tn.BackColor = this.BackColor;
                    tn.ForeColor = this.ForeColor;
                    groupNode.Nodes.Add((TreeNode)tn.Clone());
                    tn.Tag = "DELME";
                }

            this.SelectedNodes = new ArrayList();

            appendTo.Insert(pos, groupNode);

            int i = 0;
            while (i < appendTo.Count)
            {
                TreeNode tn = appendTo[i];
                if (tn.Tag == "DELME")
                {
                    tn.Remove();
                }
                else
                    i++;
            }

            this.SelectedNodes.Add(groupNode);

            this.EndUpdate();
        }

        public void UngroupNodes()
        {
            if (this.SelectedNodes.Count == 0) return;
            TreeNodeCollection appendTo = ((this.SelectedNodes[0] as TreeNode).Parent == null) ? this.Nodes : (this.SelectedNodes[0] as TreeNode).Parent.Nodes;
            foreach (TreeNode tn in this.SelectedNodes)
                if (tn.Nodes.Count > 0)
                    tn.Tag = "UNGROUP";

            this.SelectedNodes.Clear();
            Queue q = new Queue();

            foreach (TreeNode tn in appendTo)
                if (tn.Tag == "UNGROUP")
                {
                    foreach (TreeNode node in tn.Nodes)
                    {
                        //tn.Nodes.Remove(node);
                        TreeNode nodeClone = (TreeNode)node.Clone();
                        appendTo.Insert(tn.Index + node.Index + 1, nodeClone);
                        q.Enqueue(nodeClone);
                    }
                    tn.Tag = "DELME";
                }

            int i = 0;
            while (i < this.Nodes.Count)
            {
                clearNodes(this.Nodes[i]);
                i++;
            }

            this.SelectedNodes.AddRange(q);
        }

        private void clearNodes(TreeNode node)
        {
            if (node.Tag == "DELME") node.Remove();
            else
                if (node.Nodes.Count > 0)
                {
                    int i = 0;
                    while (i < node.Nodes.Count)
                    {
                        clearNodes(node.Nodes[i]);
                        i++;
                    }
                }
        }
    }
}
