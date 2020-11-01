using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;


public abstract class AtList<TCell> : MonoBehaviour where TCell : Component
{
    public GridLayoutGroup grid;
    public Transform gridTransform;
    protected List<TCell> cells = new List<TCell>(10);
    private List<TCell> pooledCells = new List<TCell>(10);
    [SerializeField]
    private TCell cellPrefab;
    public abstract int NumberOfCells();
    public abstract void UpdateCell(int index, TCell cell);

    public void Refresh()
    {
        int numCells = cells.Count;
        int numCellsRequired = NumberOfCells();
        if (numCells < numCellsRequired)
        {
            int numCellsToAdd = numCellsRequired - numCells;
            for (int x = 0; x < numCellsToAdd; x++)
            {
                AddCell();
            }
        }
        else if (numCellsRequired < numCells)
        {
            int numCellsToRem = numCells - numCellsRequired;

            for (int x = 0; x < numCellsToRem; x++)
                PushPooledCell(0);
        }
        ReloadCells();
    }

    #region Internals
    private void AddCell()
    {
        TCell cell;
        if (pooledCells.Count > 0)
        {
            cell = PopPooledCell();
        }
        else
        {
            cell = GameObject.Instantiate(cellPrefab) as TCell;
            cell.name = cell.name.Replace("(Clone)", "");
            if (grid != null)
                cell.transform.SetParent(grid.transform);
            if (gridTransform != null)
                cell.transform.SetParent(gridTransform);
        }
        int order = cells.Count;
        cell.name = string.Format("C{0}", order);
        cell.transform.localScale = Vector3.one;
        cells.Add(cell);
    }

    public void RemoveAllCells()
    {
        while (cells.Count > 0)
        {
            PushPooledCell(0);
        }
    }

    public void ClearAllCells()
    {
        cells.Clear();
        pooledCells.Clear();
        if (grid != null)
        {
            Transform gridT = grid.transform;
            for (int i = 0; i < gridT.childCount; i++)
            {
                Destroy(grid.transform.GetChild(i).gameObject);
            }
        }
        if (gridTransform != null)
        {
            for (int i = 0; i < gridTransform.childCount; i++)
            {
                Destroy(gridTransform.GetChild(i).gameObject);
            }
        }
    }

    private void ReloadCells()
    {
        for (int x = 0; x < cells.Count; x++)
        {
            UpdateCell(x, cells[x]);
        }
    }

    private void PushPooledCell(int cellIndex)
    {
        cells[cellIndex].gameObject.SetActive(false);
        pooledCells.Add(cells[cellIndex]);
        cells.RemoveAt(cellIndex);
    }

    private TCell PopPooledCell()
    {
        pooledCells[0].gameObject.SetActive(true);
        cells.Add(pooledCells[0]);
        pooledCells.RemoveAt(0);
        return cells[cells.Count - 1];
    }
    #endregion
}

