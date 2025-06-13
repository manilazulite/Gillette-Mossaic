using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityPackages.UI {
	public enum FlexDirection {
		Row,
		Column
	}

	public enum AlignSelf {
		Stretch,
		Start,
		Center,
		End
	}

	public enum JustifyContent {
		Stretch,
		Start,
		Center,
		End,
	}

	public enum ContentSpacing {
		None,
		SpaceAround,
		SpaceBetween
	}

	public enum BasisType {
		Percent,
		Pixels
	}

	[System.Serializable]
	public class FlexItem {
		public int grow = 1;
		public int basis = 0;
		public BasisType basisType = BasisType.Percent;
		public AlignSelf alignSelf = AlignSelf.Stretch;

		public float runtimeItemSize = 0;
		public RectTransform runtimeRectTransform = null;
	}

	[AddComponentMenu ("UI/Flexbox")]
	public class UIFlexbox : MonoBehaviour {

		/// The direction of the flex layout
		public FlexDirection flexDirection = FlexDirection.Row;

		// Defines how to justify the content.
		public JustifyContent justifyContent = JustifyContent.Stretch;

		/// The size of justified items.
		public int itemSize;

		// The type of content spacing
		public ContentSpacing contentSpacing = ContentSpacing.None;

		/// The spacing for Justify Content "space between" and "-around"
		public int spacing;

		//// The list of flex items.
		public List<FlexItem> flexItems = new List<FlexItem> ();

		/// Formats the child components
		public void Draw () {

			// Ensure there are the same amount of flex items
			// as there are children.
			if (this.flexItems.Count != this.transform.childCount)
				return;

			var _rectTransform = this.GetComponent<RectTransform> ();
			var _containerSize = this.GetContainerSize (_rectTransform);
			var _flexItemRelativeSize = 0f;
			var _spacing = 0f;
			var _sizing = 0f;
			var _cursor = 0f;
			var _index = -1;

			// Calculates the flex item relative size according to
			// the current justify Content mode.
			switch (this.justifyContent) {

				// ... Stretching
				case JustifyContent.Stretch:
					foreach (var _flexItem in this.flexItems)
						_flexItemRelativeSize += _flexItem.grow;
					_flexItemRelativeSize = _containerSize / _flexItemRelativeSize;
					break;

					// ... Start
				case JustifyContent.Start:
					_flexItemRelativeSize = this.itemSize;
					break;

					// ... End
				case JustifyContent.End:
					_flexItemRelativeSize = this.itemSize;
					_cursor = (_containerSize - (_flexItemRelativeSize * this.flexItems.Count));
					if (this.flexDirection == FlexDirection.Column) _cursor *= -1;
					break;

					// ... Center
				case JustifyContent.Center:
					_flexItemRelativeSize = this.itemSize;
					_cursor = (_containerSize - (_flexItemRelativeSize * this.flexItems.Count)) * .5f;
					if (this.flexDirection == FlexDirection.Column) _cursor *= -1;
					break;
			}

			// Adds spacing to the items
			switch (this.contentSpacing) {
				// ... Space Between
				case ContentSpacing.SpaceBetween:
					_spacing = this.spacing;
					_spacing -= (float) this.spacing / (float) this.flexItems.Count;
					_sizing = this.spacing;
					_sizing -= ((_sizing + _spacing) / (float) this.flexItems.Count);
					break;

					// ... Space Between
				case ContentSpacing.SpaceAround:
					_spacing = this.spacing;
					_spacing -= (float) this.spacing / (float) this.flexItems.Count;
					_sizing = this.spacing;
					_sizing -= ((_sizing - _spacing) / (float) this.flexItems.Count);
					_cursor += this.flexDirection == FlexDirection.Column ? -_spacing : _spacing;
					break;
			}

			// Now we're going to set each flex item
			foreach (var _flexItem in this.flexItems) {
				_index++;
				_flexItem.runtimeItemSize = _flexItemRelativeSize * _flexItem.grow;
				_flexItem.runtimeRectTransform = this.transform
					.GetChild (_index)
					.GetComponent<RectTransform> ();
				_flexItem.runtimeRectTransform.hideFlags = HideFlags.NotEditable;
				_flexItem.runtimeRectTransform.localScale = Vector3.one;

				// Flex Direction
				switch (this.flexDirection) {

					// ... Column
					case FlexDirection.Column:
						switch (_flexItem.alignSelf) {
							case AlignSelf.Stretch:
							case AlignSelf.Start:
								this.SetAnchorPreset (_flexItem, 0, 1);
								break;
							case AlignSelf.End:
								this.SetAnchorPreset (_flexItem, 1, 1);
								break;
							case AlignSelf.Center:
								this.SetAnchorPreset (_flexItem, .5f, 1);
								break;
						}
						_flexItem.runtimeRectTransform.anchoredPosition = new Vector3 (0, _cursor);
						_flexItem.runtimeRectTransform.SetSizeWithCurrentAnchors (
							RectTransform.Axis.Vertical,
							_flexItem.runtimeItemSize - _sizing);
						if (_flexItem.alignSelf == AlignSelf.Stretch) {
							_flexItem.runtimeRectTransform.SetSizeWithCurrentAnchors (
								RectTransform.Axis.Horizontal,
								_rectTransform.rect.width);
						} else if (_flexItem.basisType == BasisType.Percent) {
							_flexItem.runtimeRectTransform.SetSizeWithCurrentAnchors (
								RectTransform.Axis.Horizontal,
								_rectTransform.rect.width * (_flexItem.basis / 100f));
						} else if (_flexItem.basisType == BasisType.Pixels) {
							_flexItem.runtimeRectTransform.SetSizeWithCurrentAnchors (
								RectTransform.Axis.Horizontal,
								_flexItem.basis);
						}
						_cursor -= _flexItem.runtimeItemSize - _sizing;
						if (this.contentSpacing != ContentSpacing.None)
							_cursor -= _spacing;
						break;

						// ... Row
					case FlexDirection.Row:
						switch (_flexItem.alignSelf) {
							case AlignSelf.Stretch:
							case AlignSelf.Start:
								this.SetAnchorPreset (_flexItem, 0, 1);
								break;
							case AlignSelf.End:
								this.SetAnchorPreset (_flexItem, 0, 0);
								break;
							case AlignSelf.Center:
								this.SetAnchorPreset (_flexItem, 0, .5f);
								break;
						}
						_flexItem.runtimeRectTransform.anchoredPosition = new Vector3 (_cursor, 0);
						_flexItem.runtimeRectTransform.SetSizeWithCurrentAnchors (
							RectTransform.Axis.Horizontal,
							_flexItem.runtimeItemSize - _sizing);
						if (_flexItem.alignSelf == AlignSelf.Stretch) {
							_flexItem.runtimeRectTransform.SetSizeWithCurrentAnchors (
								RectTransform.Axis.Vertical,
								_rectTransform.rect.height);
						} else if (_flexItem.basisType == BasisType.Percent) {
							_flexItem.runtimeRectTransform.SetSizeWithCurrentAnchors (
								RectTransform.Axis.Vertical,
								_rectTransform.rect.height * (_flexItem.basis / 100f));
						} else if (_flexItem.basisType == BasisType.Pixels) {
							_flexItem.runtimeRectTransform.SetSizeWithCurrentAnchors (
								RectTransform.Axis.Vertical,
								_flexItem.basis);
						}
						_cursor += _flexItem.runtimeItemSize - _sizing;
						if (this.contentSpacing != ContentSpacing.None)
							_cursor += _spacing;
						break;
				}
			}
		}

		private float GetContainerSize (RectTransform rectTransform) {
			switch (this.flexDirection) {
				case FlexDirection.Row:
					return rectTransform.rect.width;
				default:
				case FlexDirection.Column:
					return rectTransform.rect.height;
			}
		}

		private void SetAnchorPreset (FlexItem flexItem, float x, float y) {
			flexItem.runtimeRectTransform.pivot =
				flexItem.runtimeRectTransform.anchorMax =
				flexItem.runtimeRectTransform.anchorMin = new Vector2 (x, y);
		}

		private void Awake () {
			this.Draw ();
		}

#if UNITY_EDITOR
		private void Update () {
			this.Draw ();
		}
#endif

		private void OnDrawGizmos () {
			if (Application.isPlaying == false)
				this.Draw ();
		}

		private void OnDrawGizmosSelected () {
			if (Application.isPlaying == false)
				this.Draw ();
			var _corners = new Vector3[4];
			foreach (var _flexItem in this.flexItems) {
				if (_flexItem.runtimeRectTransform == null)
					continue;
				_flexItem.runtimeRectTransform.GetWorldCorners (_corners);
				Gizmos.color = new Color (1, 0, 1);
				Gizmos.DrawLine (_corners[0], _corners[1]);
				Gizmos.DrawLine (_corners[1], _corners[2]);
				Gizmos.DrawLine (_corners[2], _corners[3]);
				Gizmos.DrawLine (_corners[3], _corners[0]);
			}
			this.GetComponent<RectTransform> ().GetWorldCorners (_corners);
			Gizmos.color = new Color (1, 1, 0);
			Gizmos.DrawLine (_corners[0], _corners[1]);
			Gizmos.DrawLine (_corners[1], _corners[2]);
			Gizmos.DrawLine (_corners[2], _corners[3]);
			Gizmos.DrawLine (_corners[3], _corners[0]);
		}
	}
}