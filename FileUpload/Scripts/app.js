$(function() {
	'use strict';

	window.onerror = function() {
		console.log('Window error', arguments);

		return false;
	}

	var showMessage = function showMessage(message, form) {
		$('#upload-progress').hide();
		$('#upload-alert .message').text(message);
		$('#upload-alert').show();

		if (form) form.reset();
	};

	$('form[name="upload-form"]').on('submit', function(e) {
		e.preventDefault();

		var xhr = new XMLHttpRequest(),
			data = new FormData(),
			form = e.target,
			token = form.attr('token'),
			files = $('#upload-file', form).get(0).files;

		for (var i = 0; i < files.length; i++) {
			data.append('upload-file', files[i]);
		}

		$('#upload-progress').show();

		xhr.open('post', form.action, true);

		xhr.upload.onloadstart = function(e) {
			console.log('Upload start', xhr.status, xhr.readyState, arguments);
		};

		xhr.upload.onprogress = function(e) {
			console.log('Upload progress', xhr.status, xhr.readyState, arguments);

			if (e.lengthComputable) {
				var progress = (e.loaded / e.total) * 100;

				$('#upload-progress .progress-bar').css('width', progress + '%');
			}
		};

		xhr.onreadystatechange = function(e) {
			console.log('State change', xhr.status, xhr.readyState, arguments);
		};

		xhr.upload.onloadend = function(e) {
			console.log('Upload complete', xhr.status, xhr.readyState, arguments);
		};

		xhr.onerror = function(e) {
			console.log('Error', xhr.status, xhr.readyState, arguments);

			showMessage('An error occurred while submitting the form (check file size)');
		};

		xhr.onprogress = function() {
			console.log('Progress', xhr.status, xhr.readyState, arguments);
		};

		xhr.onload = function() {
			console.log('Load', xhr.status, xhr.readyState, arguments);

			showMessage(this.statusText, form);
		};

		xhr.onabort = function() {
			console.log('Abort', xhr.status, xhr.readyState, arguments);

			showMessage('Upload aborted', form);
		};

		if (token > Date.now() + 60000) // ???
		xhr.send(data);
	}).attr('token', Date.now());

});