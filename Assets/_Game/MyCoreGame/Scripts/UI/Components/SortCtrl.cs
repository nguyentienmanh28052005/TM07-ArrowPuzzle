using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace master
{
    public delegate bool DelegateBool<T>(T _t1, T _t2);
    public static class SortCtrl
    {
        public enum ESortType
        {
            QuickSort,
        }
        public static void Sort<T>(IList<T> _sortData, DelegateBool<T> _condition, ESortType _typeSort = ESortType.QuickSort)
        {
            switch (_typeSort)
            {
                case ESortType.QuickSort:
                    new QuickSort<T>().Sort(_sortData, _condition);
                    break;
            }
        }
 
    }
    public interface ISort<T>
    {
        void Sort(IList<T> _sortData, DelegateBool<T> _condition);
    }
    #region QUICK SORT
    public class QuickSort<T> : ISort<T>
    {
        #region IList
        public void Sort(IList<T> _sortData, DelegateBool<T> _condition)
        {
            quickSort(_sortData, 0, _sortData.Count - 1, _condition);
        }
        void quickSort(IList<T> _sortData, int low, int high, DelegateBool<T> _condition)
        {
            if (low < high)
            {
                int pi = partition(_sortData, low, high, _condition);
                quickSort(_sortData, low, pi - 1, _condition);
                quickSort(_sortData, pi + 1, high, _condition);
            }
        }
        int partition(IList<T> _sortData, int low, int high, DelegateBool<T> _condition)
        {
            int pivot = high;
            int i = (low - 1);
            for (int j = low; j <= high - 1; j++)
            {
                if (_condition(_sortData[j], _sortData[pivot]))
                {
                    i++;
                    //T tmp = _sortData[j];
                    //_sortData[j] = _sortData[i];
                    //_sortData[i] = tmp;
                    _sortData.Swap(i, j);
                }
            }
            //T tmp2 = _sortData[high];
            //_sortData[high] = _sortData[i + 1];
            //_sortData[i + 1] = tmp2;
            _sortData.Swap(i + 1, high);
            return (i + 1);
        }
        #endregion
    }
    #endregion
}

