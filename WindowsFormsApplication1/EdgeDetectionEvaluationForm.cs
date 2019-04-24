using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class EdgeDetectionEvaluationForm : Form
    {
        public EdgeDetectionEvaluationForm()
        {
            InitializeComponent();
        }

        public EdgeDetectionEvaluationForm(IEnumerable<CriteriaEvaluationResult> criteriaEvaluationResults) : this()
        {
            foreach (CriteriaEvaluationResult res in criteriaEvaluationResults)
            {
                var rowIdx = dgv_edgeDetetctionEval.Rows.Add();
                dgv_edgeDetetctionEval.Rows[rowIdx].SetValues(res.name, res.worst, res.best, res.value);
            }
        }
    }
}
