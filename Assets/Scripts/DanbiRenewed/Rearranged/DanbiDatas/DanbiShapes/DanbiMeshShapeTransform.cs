using UnityEngine;

namespace Danbi {
	[System.Serializable]
	public class DanbiMeshShapeTransform : DanbiShapeTransform {
		public int indexCount;
		public int indexOffset;

		public new int stride => base.stride + 4 + 4;
	}
};