// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

// 添加全局 AJAX 错误处理
$(document).ajaxError(function (event, jqXHR, ajaxSettings, thrownError) {
    if (jqXHR.status === 401) {
        alert("登录已过期，请重新登录");
        window.location.href = '/';
    }
});

// 图片懒加载功能
document.addEventListener("DOMContentLoaded", function() {
    // 选择所有带有data-lazy属性的图片
    const lazyImages = document.querySelectorAll("img[data-lazy]");
    
    // 创建交叉观察器
    if ('IntersectionObserver' in window) {
        const imageObserver = new IntersectionObserver(function(entries, observer) {
            entries.forEach(function(entry) {
                // 当图片进入视口时
                if (entry.isIntersecting) {
                    const image = entry.target;
                    // 设置src为data-src的值
                    image.src = image.dataset.src;
                    // 移除data-lazy属性
                    image.removeAttribute('data-lazy');
                    // 图片加载完成后执行
                    image.onload = function() {
                        image.classList.add('lazy-loaded');
                    };
                    // 停止观察已加载的图片
                    imageObserver.unobserve(image);
                }
            });
        });
        
        // 开始观察每一个懒加载图片
        lazyImages.forEach(function(image) {
            imageObserver.observe(image);
        });
    } else {
        // 对于不支持IntersectionObserver的浏览器，直接加载所有图片
        lazyImages.forEach(function(image) {
            image.src = image.dataset.src;
            image.removeAttribute('data-lazy');
        });
    }
});

// 图片查看模态框功能
function showImageModal(imagePath, itemName) {
    const modalImage = document.getElementById('modalImage');
    // 添加加载指示器
    modalImage.classList.add('loading');
    // 设置标题
    document.getElementById('imageModalTitle').textContent = itemName;
    
    // 创建新图片对象预加载
    const img = new Image();
    img.onload = function() {
        // 图片加载完成后更新模态框图片并移除加载状态
        modalImage.src = imagePath;
        modalImage.classList.remove('loading');
    };
    img.onerror = function() {
        // 加载失败时显示默认图片
        modalImage.src = '/images/default-item.jpg';
        modalImage.classList.remove('loading');
    };
    // 开始加载图片
    img.src = imagePath;
}
