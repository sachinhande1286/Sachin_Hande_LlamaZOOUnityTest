using UnityEngine;

public class CControlCNode : CNode
{

    public bool active;
    public CNode above, right;

    public CControlCNode(Vector3 _pos, bool _active, float squareSize)
        : base(_pos)
    {
        active = _active;
        above = new CNode(position + Vector3.forward * squareSize / 2f);
        right = new CNode(position + Vector3.right * squareSize / 2f);
    }

}
