using System;
using Extreme.Cartesian.Green.Tensor;
using System.Collections.Generic;
using Extreme.Core;
namespace Extreme.Cartesian
{
	public class TensorCache :IDisposable
	{
		public GreenTensor gtAtoA=null;
		public  Dictionary<ObservationLevel, GreenTensor> eGreenTensors=null;
		public  Dictionary<ObservationLevel, GreenTensor> hGreenTensors=null;
		public TensorCache ()
		{
			gtAtoA = null;
			eGreenTensors = new Dictionary<ObservationLevel, GreenTensor>();
			hGreenTensors = new Dictionary<ObservationLevel, GreenTensor>();
		}
		public void Dispose()
		{
			if (gtAtoA != null)
				gtAtoA.Dispose ();
			if (eGreenTensors != null) {
				foreach (var gt in eGreenTensors.Values)
					gt.Dispose();
				eGreenTensors.Clear ();
			}
			if (hGreenTensors != null) {
				foreach (var gt in hGreenTensors.Values)
					gt.Dispose();
				hGreenTensors.Clear ();
			}
		
		}
	}


}

