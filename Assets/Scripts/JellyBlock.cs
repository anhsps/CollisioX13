using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JellyBlock : MonoBehaviour
{
    public Color blockColor { get; private set; }
    public bool remove { get; private set; }
    private Vector3Int gridPos;
    private List<JellyBlock> blocks => JellyMerge.Instance.jellyBlocks;

    private void Start()
    {
        blockColor = GetComponent<MeshRenderer>().material.color;

        gridPos = new Vector3Int(Mathf.RoundToInt(transform.position.x),
            0, Mathf.RoundToInt(transform.position.z));
        transform.position = gridPos;
    }

    public bool MoveBlock(Vector3Int direction)
    {
        Vector3Int nextPos = gridPos + direction;
        JellyBlock nextBlock = GetBlockAt(nextPos);

        while (!CheckWall(nextPos))
        {
            if (nextBlock)
            {
                if (nextBlock.blockColor == blockColor)
                {
                    remove = true;
                    StartCoroutine(Animate(nextBlock.gridPos));
                    return true;
                }
                break;// != color -> break while
            }
            // nextBlock null
            gridPos = nextPos;
            nextPos = gridPos + direction;
            nextBlock = GetBlockAt(nextPos);
        }
        // != color || CheckWall
        if (gridPos != transform.position)
        {
            StartCoroutine(Animate(gridPos));
            return true;
        }

        return false;
    }

    private JellyBlock GetBlockAt(Vector3Int pos)
    {
        foreach (var block in blocks)
            if (block.gridPos == pos && block != this) return block;
        return null;
    }

    private bool CheckWall(Vector3Int nextPos)
    {
        return Physics.CheckSphere(nextPos, 0.1f, LayerMask.GetMask("Ground"));
    }

    public int ComparePosition(Vector3Int moveDir)
    {// vd: move right -> gridPos.x lon hon se move truoc nen de -gridPos.x de sx truoc
        return moveDir.x != 0 ? -gridPos.x * moveDir.x : -gridPos.z * moveDir.z;
    }

    private IEnumerator Animate(Vector3 to)
    {
        Vector3 from = transform.position;
        float elapsed = 0;
        float duration = 0.1f;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = to;
    }
}