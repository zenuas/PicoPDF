"use strict";

import Cookies from "https://cdn.jsdelivr.net/npm/js-cookie@3.0.1/dist/js.cookie.min.mjs";
import * as TableSort from "./table-sort.js";

window.addEventListener("load", () => {
	document.querySelectorAll("table:not(.table-sort)").forEach(x => TableSort.attach(x));
	
	document.querySelectorAll("details[data-auto-cookie]").forEach(details => {
		const id    = details.dataset.autoCookie;
		const value = Cookies.get(id);
		if(value == "close") details.open = false;
		details.addEventListener("toggle", () => Cookies.set(id, details.open ? "open" : "close", { expires: 7 }));
	});
	
	const input_set_value = (input, value) => {
		if(input.type == "checkbox" || input.type == "radio")
		{
			input.checked = value == "true";
		}
		else
		{
			input.value = value;
		}
		input.dispatchEvent(new Event("input",  {bubbles: true, cancelable: true}));
		input.dispatchEvent(new Event("change", {bubbles: true, cancelable: true}));
	};
	
	const url = (new URL(window.location.href)).searchParams;
	document.querySelectorAll("input[data-auto-param], select[data-auto-param], textarea[data-auto-param]").forEach(input => {
		const id    = input.dataset.autoParam;
		const param = url.get(id);
		if(param != null)
		{
			input_set_value(input, param);
		}
	});
	
	document.querySelectorAll("input[data-auto-cookie], select[data-auto-cookie], textarea[data-auto-cookie]").forEach(input => {
		const id    = "data-auto-cookie" + (
			input.dataset.autoCookie != undefined && input.dataset.autoCookie != "" ? input.dataset.autoCookie :
			input.name               != undefined && input.name               != "" ? input.name :
			input.id
			);
		const value = Cookies.get(id);
		if(value != undefined && value != "")
		{
			input_set_value(input, value);
		}
		
		if(input.type == "checkbox" || input.type == "radio")
		{
			input.addEventListener("change", () => Cookies.set(id, input.checked, { expires: 7 }));
		}
		else
		{
			input.addEventListener("change", () => Cookies.set(id, input.value, { expires: 7 }));
		}
	});
	
	document.querySelectorAll("input[type=number]").forEach(input => {
		const min  = parseFloat(input.min);
		const max  = parseFloat(input.max);
		const step = input.step;
		
		const enable_e     = false;
		const enable_minus = !(!isNaN(min) && min >= 0);
		const enable_point = !(step != "" && step.indexOf(".") < 0);
		
		input.addEventListener("input", () => {
			const v = parseFloat(input.value);
			if(isNaN(v))
			{
				if(input.required) input.value = isNaN(min) ? 0 : min;
				return;
			}
			
			if(!isNaN(min) && min > v) input.value = min;
			if(!isNaN(max) && max < v) input.value = max;
		});
		
		input.addEventListener("keydown", e => {
			if(!enable_e && e.keyCode == 69) return false;
			if(!enable_minus && (e.keyCode == 189 || e.keyCode == 109)) return false;
			if(!enable_point && (e.keyCode == 190 || e.keyCode == 110)) return false;
			return true;
		});
	});
});
