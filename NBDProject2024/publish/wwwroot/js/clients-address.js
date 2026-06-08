(function () {
  function initClientAddressForm() {
    if (!window.jQuery) {
      return;
    }

    var $form = $("form[data-client-address='true']");
    if ($form.length === 0) {
      return;
    }

    var $useNewCity = $("#UseNewCity");
    var $citySelect = $("#CityID");
    var $newCityInput = $("#NewCityName");
    var $province = $("#ProvinceID");
    var defaultCountry = $form.data("defaultCountry");

    function toggleCityMode() {
      var useNewCity = $useNewCity.is(":checked");
      $citySelect.prop("disabled", useNewCity);
      $newCityInput.prop("disabled", !useNewCity);
    }

    if (defaultCountry) {
      var $country = $("#AddressCountry");
      if ($country.length && !$country.val()) {
        $country.val(defaultCountry);
      }
    }

    toggleCityMode();

    $useNewCity.on("change", toggleCityMode);

    $province.on("change", function () {
      var selectedProvince = $province.val();
      var url = "/Clients/GetCities/?ProvinceID=" + selectedProvince;
      if (typeof refreshDDL === "function") {
        refreshDDL(
          "CityID",
          url,
          true,
          "Select a Province with Cities first",
          false,
          null,
          true,
        );
      }
    });
  }

  if (window.jQuery) {
    $(document).ready(initClientAddressForm);
  }
})();
