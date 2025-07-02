using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class HorizontalScrollController : MonoBehaviour, IScrollHandler
{
	[Header("Scroll Settings")]
	[SerializeField] private float scrollSpeed = 100f;
	[SerializeField] private ScrollRect scrollRect;

	[Header("Smooth Scrolling")]
	[SerializeField] private bool useSmoothScrolling = true;
	[SerializeField] private float smoothTime = 0.1f;

	private float targetHorizontalPosition;
	private float velocity;

	void Start()
	{
		// Get ScrollRect component if not assigned
		if (scrollRect == null)
			scrollRect = GetComponent<ScrollRect>();

		// Ensure horizontal scrolling is enabled and vertical is disabled
		if (scrollRect != null)
		{
			scrollRect.horizontal = true;
			scrollRect.vertical = false;

			// Hide scrollbars
			if (scrollRect.horizontalScrollbar != null)
				scrollRect.horizontalScrollbar.gameObject.SetActive(false);
			if (scrollRect.verticalScrollbar != null)
				scrollRect.verticalScrollbar.gameObject.SetActive(false);
		}

		targetHorizontalPosition = scrollRect.horizontalNormalizedPosition;
	}

	void Update()
	{
		if (useSmoothScrolling && scrollRect != null)
		{
			// Smooth scrolling
			scrollRect.horizontalNormalizedPosition = Mathf.SmoothDamp(
				scrollRect.horizontalNormalizedPosition,
				targetHorizontalPosition,
				ref velocity,
				smoothTime
			);
		}
	}

	public void OnScroll(PointerEventData eventData)
	{
		if (scrollRect == null) return;

		// Calculate scroll delta
		float scrollDelta = eventData.scrollDelta.y * scrollSpeed * Time.unscaledDeltaTime;

		if (useSmoothScrolling)
		{
			// Update target position for smooth scrolling
			targetHorizontalPosition = Mathf.Clamp01(targetHorizontalPosition + scrollDelta);
		}
		else
		{
			// Direct scrolling
			scrollRect.horizontalNormalizedPosition = Mathf.Clamp01(
				scrollRect.horizontalNormalizedPosition + scrollDelta
			);
		}
	}

	// Public methods for external control
	public void SetScrollSpeed(float newSpeed)
	{
		scrollSpeed = newSpeed;
	}

	public void ScrollToPosition(float normalizedPosition)
	{
		normalizedPosition = Mathf.Clamp01(normalizedPosition);

		if (useSmoothScrolling)
		{
			targetHorizontalPosition = normalizedPosition;
		}
		else
		{
			scrollRect.horizontalNormalizedPosition = normalizedPosition;
		}
	}

	public void ScrollToBeginning()
	{
		ScrollToPosition(0f);
	}

	public void ScrollToEnd()
	{
		ScrollToPosition(1f);
	}
}