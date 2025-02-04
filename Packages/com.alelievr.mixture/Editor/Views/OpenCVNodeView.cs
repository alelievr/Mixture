using GraphProcessor;
using Mixture.OpenCV;
using OpenCvSharp;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mixture
{
    [NodeCustomEditor(typeof(OpenCVNode))]
    public class OpenCVNodeView : MixtureNodeView
    {
        public override void Enable(bool fromInspector)
        {
            base.Enable(fromInspector);
            foreach (var port in this.portsPerFieldName.Values)
            {
                foreach (var p in port)
                {
                    Debug.Log(p);
                    if (p.fieldType == typeof(Mat))
                    {
                        p.focusable = true;
                        p.AddManipulator(new ContextualMenuManipulator(evt =>
                        {
                            evt.menu.AppendAction("ImShow", action =>
                            {
                                Cv2.ImShow(p.fieldName, (Mat)p.GetValue<Mat>(this.nodeTarget));
                            });
                        }));
                    }
                }
            }
        }
    }
}