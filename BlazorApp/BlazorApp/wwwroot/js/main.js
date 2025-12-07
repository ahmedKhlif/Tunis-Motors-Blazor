(function($) {
	"use strict"

	// Mobile Nav toggle
	$('.menu-toggle > a').on('click', function (e) {
		e.preventDefault();
		$('#responsive-nav').toggleClass('active');
	})

	// Fix cart dropdown from closing
	$('.cart-dropdown').on('click', function (e) {
		e.stopPropagation();
	});

	/////////////////////////////////////////

	// Products Slick
	$('.products-slick').each(function() {
		var $this = $(this),
				$nav = $this.attr('data-nav');

		$this.slick({
			slidesToShow: 4,
			slidesToScroll: 1,
			autoplay: true,
			infinite: true,
			speed: 300,
			dots: false,
			arrows: true,
			appendArrows: $nav ? $nav : false,
			responsive: [{
	        breakpoint: 991,
	        settings: {
	          slidesToShow: 2,
	          slidesToScroll: 1,
	        }
	      },
	      {
	        breakpoint: 480,
	        settings: {
	          slidesToShow: 1,
	          slidesToScroll: 1,
	        }
	      },
	    ]
		});
	});

	// Products Widget Slick
	$('.products-widget-slick').each(function() {
		var $this = $(this),
				$nav = $this.attr('data-nav');

		$this.slick({
			infinite: true,
			autoplay: true,
			speed: 300,
			dots: false,
			arrows: true,
			appendArrows: $nav ? $nav : false,
		});
	});

	/////////////////////////////////////////

	// Home Slick
	$('#home-slick').slick({
		autoplay: true,
		infinite: true,
		speed: 500,
		arrows: true,
		dots: true,
		fade: true,
		cssEase: 'linear'
	});

	/////////////////////////////////////////

	// Product Main img Slick
	$('#product-main-img').slick({
    infinite: true,
    speed: 300,
    dots: false,
    arrows: true,
    fade: true,
    asNavFor: '#product-imgs',
  });

	// Product imgs Slick
  $('#product-imgs').slick({
    slidesToShow: 3,
    slidesToScroll: 1,
    arrows: true,
    centerMode: true,
    focusOnSelect: true,
		centerPadding: 0,
		vertical: true,
    asNavFor: '#product-main-img',
		responsive: [{
        breakpoint: 991,
        settings: {
					vertical: false,
					arrows: false,
					dots: true,
        }
      },
    ]
  });

	// Product img zoom
	var zoomMainProduct = document.getElementById('product-main-img');
	if (zoomMainProduct) {
		$('#product-main-img .product-preview').zoom();
	}

	/////////////////////////////////////////

	// Input number
	$('.input-number').each(function() {
		var $this = $(this),
		$input = $this.find('input[type="number"]'),
		up = $this.find('.qty-up'),
		down = $this.find('.qty-down');

		down.on('click', function () {
			var value = parseInt($input.val()) - 1000;
			value = value < 0 ? 0 : value;
			$input.val(value);
			$input.change();
			updatePriceSlider($this , value)
		})

		up.on('click', function () {
			var value = parseInt($input.val()) + 1000;
			value = value > 500000 ? 500000 : value;
			$input.val(value);
			$input.change();
			updatePriceSlider($this , value)
		})
	});

	var priceInputMax = document.getElementById('price-max'),
			priceInputMin = document.getElementById('price-min');

	if (priceInputMax) {
		priceInputMax.addEventListener('change', function(){
			updatePriceSlider($(this).parent() , this.value)
		});
	}

	if (priceInputMin) {
		priceInputMin.addEventListener('change', function(){
			updatePriceSlider($(this).parent() , this.value)
		});
	}

	function updatePriceSlider(elem , value) {
		if ( elem.hasClass('price-min') ) {
			console.log('min')
			if (priceSlider && priceSlider.noUiSlider) {
				var currentValues = priceSlider.noUiSlider.get();
				priceSlider.noUiSlider.set([value, currentValues[1]]);
			}
		} else if ( elem.hasClass('price-max')) {
			console.log('max')
			if (priceSlider && priceSlider.noUiSlider) {
				var currentValues = priceSlider.noUiSlider.get();
				priceSlider.noUiSlider.set([currentValues[0], value]);
			}
		}
	}

	// Price Slider
	var priceSlider = document.getElementById('price-slider');
	if (priceSlider) {
		noUiSlider.create(priceSlider, {
			start: [0, 500000],
			connect: true,
			step: 1000,
			range: {
				'min': 0,
				'max': 500000
			},
			format: {
				to: function (value) {
					return Math.round(value);
				},
				from: function (value) {
					return Number(value);
				}
			}
		});

		priceSlider.noUiSlider.on('update', function( values, handle ) {
			var value = values[handle];
			handle ? priceInputMax.value = value : priceInputMin.value = value
		});
	}

})(jQuery);

// Bootstrap Carousel control for Blazor
window.goToCarouselSlide = function(carouselId, slideIndex) {
	try {
		var carouselElement = document.getElementById(carouselId);
		if (!carouselElement) {
			console.error('Carousel element not found:', carouselId);
			return;
		}

		// Check if Bootstrap is available
		if (typeof bootstrap === 'undefined' || !bootstrap.Carousel) {
			console.error('Bootstrap Carousel is not available');
			return;
		}

		var carousel = bootstrap.Carousel.getInstance(carouselElement);
		if (!carousel) {
			carousel = new bootstrap.Carousel(carouselElement, {
				interval: false,
				wrap: true
			});
		}
		carousel.to(slideIndex);
	} catch (error) {
		console.error('Error in goToCarouselSlide:', error);
	}
};
