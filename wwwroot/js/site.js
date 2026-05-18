function openComments(postId) {
  document.getElementById('comments-modal-' + postId).classList.add('show');
  document.body.style.overflow = 'hidden';
}

function closeComments(postId) {
  document.getElementById('comments-modal-' + postId).classList.remove('show');
  document.body.style.overflow = '';
}

document.addEventListener('click', function (e) {
  if (e.target.classList.contains('comment-modal-overlay')) {
    e.target.classList.remove('show');
    document.body.style.overflow = '';
  }
});

document.addEventListener('keydown', function (e) {
  if (e.key === 'Escape') {
    document.querySelectorAll('.comment-modal-overlay.show').forEach(function (el) {
      el.classList.remove('show');
    });
    document.body.style.overflow = '';
  }
});
