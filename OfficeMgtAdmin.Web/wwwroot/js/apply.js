function showApplyModal(itemId, itemName, currentStock) {
    $('#itemId').val(itemId);
    $('#itemName').val(itemName);
    $('#currentStock').val(currentStock);
    $('#applyNum').val('').attr('max', currentStock);
    $('#applyModal').modal('show');
}

function submitApply() {
    var itemId = $('#itemId').val();
    var applyNum = $('#applyNum').val();

    if (!applyNum || applyNum < 1) {
        alert('请输入有效的申请数量');
        return;
    }

    $.ajax({
        url: '/Home/SubmitApply',
        type: 'POST',
        data: {
            itemId: itemId,
            applyNum: applyNum
        },
        success: function (response) {
            if (response.success) {
                alert('申请提交成功');
                $('#applyModal').modal('hide');
                // 刷新物品列表
                $('#queryBtn').click();
            } else {
                alert(response.message || '申请提交失败');
            }
        },
        error: function () {
            alert('申请提交失败，请稍后重试');
        }
    });
} 