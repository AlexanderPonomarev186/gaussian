using System;
using System.Collections.Generic;
using System.Linq;

namespace GaussAlgorithm
{
    public class Solver
    {
        public static void CopyMatrix(double[][] matrix, double[] freemembers,
                    ref double[][] mtxcopy, ref double[] freecopy)
        {
            for (var i = 0; i < matrix.Length; i++)
            {
                mtxcopy[i] = new double[matrix[0].Length];
                matrix[i].CopyTo(mtxcopy[i], 0);
            }
            freemembers.CopyTo(freecopy, 0);
        }

        public static double[] MatrixIs1x1(double[][] matrix, double[] freeMembers)
        {
            for (int i = 0; i < matrix.Length; i++)
            {
                if (freeMembers[i] != 0 && matrix[i][0] != 0)
                {
                    freeMembers[i] /= matrix[i][0];
                    matrix[i][0] = matrix[i][0] / matrix[i][0];
                }
            }
            if (matrix.All(x => x.Length == 1)
                && !freeMembers.All(x => x == freeMembers[0]))
                throw new NoSolutionException("");
            return freeMembers;
        }

        public static double[] MatrixCheck(int rowCount, int columnsCount,
                ref double[][] matrix, ref double[] freeMembers)
        {
            if (rowCount != columnsCount)
                if (matrix.All(x => x.Length == 1))
                {
                    return MatrixIs1x1(matrix, freeMembers);
                }
            if (rowCount < columnsCount)
            {
                var mtxLength = matrix.Length;
                for (int i = 0; i < columnsCount - rowCount; i++)
                {
                    matrix = matrix.Append(new double[matrix[0].Length]).ToArray();
                    freeMembers = freeMembers.Append(0).ToArray();
                }
            }
            return null;
        }

        public static double[] GetSolution(double[][] mtxMatrix, double[] mtxfreemembers, double eps)
        {
            for (int i = 0; i < mtxMatrix[0].Length; i++)
            {
                if (mtxMatrix[i].All(x => Math.Abs(x) < eps))
                    if (Math.Abs(mtxfreemembers[i]) > eps)
                        throw new NoSolutionException("");
                if (mtxMatrix[i][i] != 1)
                {
                    for (var j = 0; j < mtxMatrix.Length; j++)
                        if (mtxMatrix[j][i] == 1)
                        {
                            (mtxMatrix[i], mtxMatrix[j]) = (mtxMatrix[j], mtxMatrix[i]);
                            (mtxfreemembers[i], mtxfreemembers[j]) = (mtxfreemembers[j], mtxfreemembers[i]);
                        }
                }
            }
            return mtxfreemembers;
        }

        public double[] Solve(double[][] matrix, double[] freemembers)
        {
            double eps = 1E-10;
            int rowCount = matrix.Length;
            int columnsCount = matrix[0].Length;
            var mtxmatrix = new double[matrix.Length][];
            var mtxfreemembers = new double[freemembers.Length];
            CopyMatrix(matrix, freemembers, ref mtxmatrix, ref mtxfreemembers);
            double[] freeMembers1x1 = MatrixCheck(rowCount, columnsCount, ref mtxmatrix, ref mtxfreemembers);
            if (freeMembers1x1 != null) return freeMembers1x1;
            List<double[]> listOfMtx = mtxmatrix.ToList();
            List<int> indexesOfRemovedRows = new List<int>();
            for (int i = 0; i < columnsCount; i++)
            {
                var tempRow = listOfMtx.FirstOrDefault(x => Math.Abs(x[i]) > eps);
                if (tempRow == null) continue;
                var tempNum = tempRow[i];
                var matrixIndex = mtxmatrix.ToList().IndexOf(tempRow);
                if (tempNum != 0)
                {
                    mtxmatrix[matrixIndex] = mtxmatrix[matrixIndex].Select(x => (double)(x / tempNum)).ToArray();
                    tempRow = tempRow.Select(x => (double)(x / tempNum)).ToArray();
                    mtxfreemembers[matrixIndex] = mtxfreemembers[matrixIndex] / tempNum;
                }
                for (var j = 0; j < mtxmatrix.Length; j++)
                {
                    var tempFirstElem = -mtxmatrix[j][i];
                    if (matrixIndex == j) continue;
                    for (var k = 0; k < mtxmatrix[j].Length; k++)
                        mtxmatrix[j][k] += tempRow[k] * tempFirstElem;
                    mtxfreemembers[j] += mtxfreemembers[matrixIndex] * tempFirstElem;
                }
                indexesOfRemovedRows.Add(matrixIndex);
                listOfMtx = mtxmatrix.ToList();
                listOfMtx = listOfMtx.Where(x => !indexesOfRemovedRows.Contains(listOfMtx.IndexOf(x))).ToList();
            }
            return GetSolution(mtxmatrix, mtxfreemembers, eps);
        }
    }
}