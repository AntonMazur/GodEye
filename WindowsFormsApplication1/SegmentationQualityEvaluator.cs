using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApplication1
{
    public struct CriteriaEvaluationResult
    {
        public string name;
        public double worst;
        public double best;
        public double value;

        public CriteriaEvaluationResult(string name, double worst, double best, double value)
        {
            this.name = name;
            this.worst = worst;
            this.best = best;
            this.value = value;
        }
    }

    class SegmentationQualityEvaluator
    {

        public static double evaluatePRA(bool[,] groundtruth, bool[,] segmResult)
        {
            var width = groundtruth.GetLength(1);
            var height = groundtruth.GetLength(0);
            double res = 1 / Math.Max(countTrueCells(groundtruth), countTrueCells(segmResult));
            double derivaitonAcc = 0; 

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (segmResult[y, x])
                    {
                        derivaitonAcc += 1 / (1 + Math.Pow(getClosestEdgePixel(x, y, groundtruth), 2));
                    }
                }
            }

            return res * derivaitonAcc;
        }

        private static int countTrueCells(bool[,] matrix)
        {
            var count = 0;

            foreach (var el in matrix)
            {
                if (el) count++;
            }

            return count;
        }


        private static int getClosestEdgePixel(int x, int y, bool[,] gtI)
        {
            if (gtI[y, x]) return 0;

            var width = gtI.GetLength(0);
            var height = gtI.GetLength(1);
            var maxDistance = Math.Max(Math.Max(x, width - x - 1), Math.Max(y, height - y - 1));

            for (var currDistance = 1; currDistance < maxDistance; currDistance++)
            {
                var startX = Math.Max(x - currDistance, 0);
                var endX = Math.Min(x + currDistance, width - 1);
                var startY = Math.Max(y - currDistance, 0);
                var endY = Math.Min(y + currDistance, height - 1);

                for (; startY <= endY; startY++)
                {
                    for (; startX <= endX; startX++)
                    {
                        if (gtI[startY, startX]) return Math.Abs(x - startX) + Math.Abs(y - startY);
                    }
                }
            }

            return int.MaxValue;
        }
    }
}
