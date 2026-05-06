// StudySpace Admin SPA
const API = '/api';
let token = localStorage.getItem('ss_token');
let currentUser = JSON.parse(localStorage.getItem('ss_user') || 'null');
let myGroupsCache = [];

// ---------- API helper ----------
async function api(path, opts = {}) {
    const headers = { ...(opts.headers || {}) };
    if (!(opts.body instanceof FormData)) {
        headers['Content-Type'] = headers['Content-Type'] || 'application/json';
    }
    if (token) headers['Authorization'] = `Bearer ${token}`;
    const res = await fetch(API + path, { ...opts, headers });
    let data;
    try { data = await res.json(); } catch { data = { success: false, message: `HTTP ${res.status}` }; }
    if (!res.ok) throw new Error(data?.message || `HTTP ${res.status}`);
    return data;
}

function fmtDate(d) {
    if (!d) return '';
    const dt = new Date(d);
    return dt.toLocaleString('vi-VN', { day: '2-digit', month: '2-digit', year: 'numeric', hour: '2-digit', minute: '2-digit' });
}

function fmtDateOnly(d) {
    if (!d) return '';
    return new Date(d).toLocaleDateString('vi-VN');
}

function fmtBytes(n) {
    if (!n) return '0 B';
    const u = ['B','KB','MB','GB'];
    let i = 0; let v = n;
    while (v >= 1024 && i < u.length - 1) { v /= 1024; i++; }
    return `${v.toFixed(v < 10 ? 1 : 0)} ${u[i]}`;
}

function alertBox(target, type, msg) {
    document.getElementById(target).innerHTML = `<div class="alert alert-${type}">${msg}</div>`;
    if (type === 'success' || type === 'info') {
        setTimeout(() => { const el = document.getElementById(target); if (el) el.innerHTML = ''; }, 3000);
    }
}

function escapeHtml(s) {
    if (s == null) return '';
    return String(s)
        .replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;').replace(/'/g, '&#039;');
}

// ---------- Auth ----------
function switchTab(which) {
    document.getElementById('tabLogin').classList.toggle('active', which === 'login');
    document.getElementById('tabRegister').classList.toggle('active', which === 'register');
    document.getElementById('loginForm').style.display = which === 'login' ? '' : 'none';
    document.getElementById('registerForm').style.display = which === 'register' ? '' : 'none';
    document.getElementById('loginAlert').innerHTML = '';
}

async function login() {
    const email = document.getElementById('loginEmail').value.trim();
    const password = document.getElementById('loginPassword').value;
    if (!email || !password) return alertBox('loginAlert', 'danger', 'Vui lòng nhập email và mật khẩu');
    try {
        const r = await api('/auth/login', { method: 'POST', body: JSON.stringify({ email, password }) });
        token = r.data.token;
        currentUser = r.data;
        localStorage.setItem('ss_token', token);
        localStorage.setItem('ss_user', JSON.stringify(currentUser));
        showApp();
    } catch (e) {
        alertBox('loginAlert', 'danger', e.message);
    }
}

async function register() {
    const fullName = document.getElementById('regName').value.trim();
    const email = document.getElementById('regEmail').value.trim();
    const phone = document.getElementById('regPhone').value.trim();
    const password = document.getElementById('regPassword').value;
    if (!fullName || !email || !phone || !password)
        return alertBox('loginAlert', 'danger', 'Vui lòng điền đầy đủ thông tin');
    try {
        const r = await api('/auth/register', { method: 'POST', body: JSON.stringify({ fullName, email, phone, password }) });
        token = r.data.token;
        currentUser = r.data;
        localStorage.setItem('ss_token', token);
        localStorage.setItem('ss_user', JSON.stringify(currentUser));
        showApp();
    } catch (e) {
        alertBox('loginAlert', 'danger', e.message);
    }
}

function logout() {
    localStorage.removeItem('ss_token');
    localStorage.removeItem('ss_user');
    token = null; currentUser = null;
    document.getElementById('loginPage').style.display = '';
    document.getElementById('mainLayout').style.display = 'none';
}

function showApp() {
    document.getElementById('loginPage').style.display = 'none';
    document.getElementById('mainLayout').style.display = 'flex';
    document.getElementById('currentUser').innerHTML = `<i class="fas fa-user-circle"></i> ${escapeHtml(currentUser.fullName)} (${currentUser.role})`;
    if (currentUser.role === 'Admin') document.getElementById('navUsers').style.display = '';
    showPage('dashboard');
    refreshUnreadBadge();
}

// ---------- Page navigation ----------
function showPage(page) {
    document.querySelectorAll('.page-content').forEach(p => p.style.display = 'none');
    document.querySelectorAll('.nav-item').forEach(n => n.classList.remove('active'));
    document.getElementById(`page-${page}`).style.display = '';
    const nav = document.querySelector(`.nav-item[data-page="${page}"]`);
    if (nav) nav.classList.add('active');

    if (page === 'dashboard') loadDashboard();
    if (page === 'groups') loadGroups();
    if (page === 'documents') loadDocumentGroupsAndList();
    if (page === 'schedules') loadScheduleGroupsAndList();
    if (page === 'progress') loadProgressGroupsAndList();
    if (page === 'chat') loadChatGroups();
    if (page === 'notifications') loadNotifications();
    if (page === 'users') loadUsers();
}

// ---------- Dashboard ----------
async function loadDashboard() {
    try {
        if (currentUser?.role !== 'Admin') {
            const groups = await api('/groups/mine');
            myGroupsCache = groups.data;
            const upcoming = await api('/schedules/upcoming?days=14');
            const docs = await Promise.all(myGroupsCache.map(g => api(`/documents/by-group/${g.id}`).catch(() => ({ data: [] }))));
            const totalDocs = docs.reduce((s, r) => s + r.data.length, 0);
            const stats = [
                { icon: 'fa-users', cls: 'primary', label: 'Nhóm của tôi', value: myGroupsCache.length },
                { icon: 'fa-calendar-day', cls: 'info', label: 'Lịch sắp tới', value: upcoming.data.length },
                { icon: 'fa-file-alt', cls: 'success', label: 'Tài liệu trong nhóm', value: totalDocs },
                { icon: 'fa-bell', cls: 'warning', label: 'Thông báo chưa đọc', value: '...' }
            ];
            document.getElementById('dashboardStats').innerHTML = stats.map(s => `
                <div class="stat-card">
                    <div class="stat-icon ${s.cls}"><i class="fas ${s.icon}"></i></div>
                    <div class="stat-info"><div class="label">${s.label}</div><div class="value">${s.value}</div></div>
                </div>`).join('');
            const unread = await api('/notifications/count-unread');
            document.querySelector('#dashboardStats .stat-card:last-child .value').textContent = unread.data;

            renderUpcomingForUser(upcoming.data);
            return;
        }

        const r = await api('/dashboard/summary');
        const d = r.data;
        const stats = [
            { icon: 'fa-users', cls: 'primary', label: 'Người dùng', value: d.totalUsers },
            { icon: 'fa-user-graduate', cls: 'success', label: 'Nhóm học', value: d.totalGroups },
            { icon: 'fa-file-alt', cls: 'info', label: 'Tài liệu', value: d.totalDocuments },
            { icon: 'fa-calendar-day', cls: 'warning', label: 'Lịch sắp tới', value: d.upcomingSchedules },
            { icon: 'fa-comments', cls: 'primary', label: 'Tin nhắn 7 ngày', value: d.messagesLast7Days },
            { icon: 'fa-user-plus', cls: 'success', label: 'User mới (7 ngày)', value: d.newUsersLast7Days }
        ];
        document.getElementById('dashboardStats').innerHTML = stats.map(s => `
            <div class="stat-card">
                <div class="stat-icon ${s.cls}"><i class="fas ${s.icon}"></i></div>
                <div class="stat-info"><div class="label">${s.label}</div><div class="value">${s.value}</div></div>
            </div>`).join('');

        document.getElementById('topGroupsTable').innerHTML = renderTable(
            ['Nhóm', 'Thành viên', 'Tài liệu', 'Tin nhắn'],
            d.topGroups.map(g => [escapeHtml(g.groupName), g.memberCount, g.documentCount, g.messageCount])
        );

        document.getElementById('activityTable').innerHTML = renderTable(
            ['Ngày', 'Tin nhắn', 'Tài liệu mới', 'Thành viên mới'],
            d.activityLast7Days.map(a => [fmtDateOnly(a.date), a.newMessages, a.newDocuments, a.newMembers])
        );
    } catch (e) {
        document.getElementById('dashboardStats').innerHTML = `<div class="alert alert-danger">${e.message}</div>`;
    }
}

function renderUpcomingForUser(list) {
    document.getElementById('topGroupsTable').innerHTML = list.length === 0
        ? `<div class="empty-state"><i class="fas fa-calendar"></i><p>Bạn chưa có lịch học sắp tới.</p></div>`
        : renderTable(
            ['Nhóm', 'Tiêu đề', 'Bắt đầu', 'Kết thúc', 'Trạng thái'],
            list.map(s => [
                escapeHtml(s.groupName),
                escapeHtml(s.title),
                fmtDate(s.startTime),
                fmtDate(s.endTime),
                `<span class="badge info">${s.status}</span>`
            ]));
    document.getElementById('activityTable').innerHTML = '';
}

function renderTable(headers, rows) {
    if (!rows || rows.length === 0)
        return `<div class="empty-state"><i class="fas fa-inbox"></i><p>Không có dữ liệu</p></div>`;
    return `<table class="data-table"><thead><tr>${headers.map(h => `<th>${h}</th>`).join('')}</tr></thead>
        <tbody>${rows.map(r => `<tr>${r.map(c => `<td>${c ?? ''}</td>`).join('')}</tr>`).join('')}</tbody></table>`;
}

// ---------- Groups ----------
async function loadGroups() {
    try {
        const search = document.getElementById('groupSearch')?.value?.trim() || '';
        const r = await api(`/groups?search=${encodeURIComponent(search)}`);
        const rows = r.data.map(g => [
            `<strong>${escapeHtml(g.name)}</strong><div style="font-size:0.85rem;color:var(--text-muted)">${escapeHtml(g.subject)}</div>`,
            escapeHtml(g.createdByName),
            `${g.memberCount}/${g.maxMembers}`,
            g.documentCount,
            `<code>${escapeHtml(g.inviteCode)}</code>`,
            g.isMember ? `<span class="badge success">${g.myRole}</span>` : (g.isPublic ? `<span class="badge info">Công khai</span>` : `<span class="badge secondary">Riêng tư</span>`),
            renderGroupActions(g)
        ]);
        document.getElementById('groupsTable').innerHTML = renderTable(
            ['Nhóm', 'Người tạo', 'Thành viên', 'Tài liệu', 'Mã mời', 'Trạng thái', 'Thao tác'],
            rows);
    } catch (e) {
        document.getElementById('groupsTable').innerHTML = `<div class="alert alert-danger">${e.message}</div>`;
    }
}

function renderGroupActions(g) {
    const canModify = g.myRole === 'Leader' || currentUser?.role === 'Admin';
    let html = `<button class="btn btn-sm btn-outline" onclick="viewGroup(${g.id})"><i class="fas fa-eye"></i></button> `;
    if (g.isMember && g.myRole !== 'Leader') {
        html += `<button class="btn btn-sm btn-secondary" onclick="leaveGroup(${g.id})"><i class="fas fa-sign-out-alt"></i> Rời</button> `;
    }
    if (canModify) {
        html += `<button class="btn btn-sm btn-warning" onclick="editGroup(${g.id})"><i class="fas fa-edit"></i></button> `;
        html += `<button class="btn btn-sm btn-danger" onclick="deleteGroup(${g.id})"><i class="fas fa-trash"></i></button>`;
    }
    return html;
}

async function viewGroup(id) {
    try {
        const r = await api(`/groups/${id}`);
        const g = r.data;
        const m = await api(`/groups/${id}/members`);
        const memberRows = m.data.map(x => `<tr>
            <td>${escapeHtml(x.fullName)}</td>
            <td>${escapeHtml(x.email)}</td>
            <td><span class="badge ${x.role === 'Leader' ? 'primary' : 'secondary'}">${x.role}</span></td>
            <td>${fmtDate(x.joinedAt)}</td>
        </tr>`).join('');
        openModal('Thông tin nhóm', `
            <p><strong>Tên:</strong> ${escapeHtml(g.name)}</p>
            <p><strong>Môn học:</strong> ${escapeHtml(g.subject)}</p>
            <p><strong>Mô tả:</strong> ${escapeHtml(g.description) || '-'}</p>
            <p><strong>Mã mời:</strong> <code>${escapeHtml(g.inviteCode)}</code></p>
            <p><strong>Thành viên:</strong> ${g.memberCount}/${g.maxMembers}</p>
            <hr style="margin:1rem 0">
            <h3 style="margin-bottom:0.5rem;font-size:1rem">Thành viên</h3>
            <table class="data-table"><thead><tr><th>Tên</th><th>Email</th><th>Vai trò</th><th>Tham gia</th></tr></thead>
            <tbody>${memberRows}</tbody></table>
        `, []);
    } catch (e) { alert(e.message); }
}

function showCreateGroupModal() {
    openModal('Tạo nhóm học mới', `
        <div class="form-group"><label>Tên nhóm *</label><input id="g_name" class="form-control"></div>
        <div class="form-group"><label>Môn học *</label><input id="g_subject" class="form-control" placeholder="Ví dụ: Toán, Lập trình"></div>
        <div class="form-group"><label>Mô tả</label><textarea id="g_desc" class="form-control"></textarea></div>
        <div class="form-row">
            <div class="form-group"><label>Số thành viên tối đa</label><input id="g_max" class="form-control" type="number" value="50" min="2"></div>
            <div class="form-group"><label>Hiển thị</label>
                <select id="g_public" class="form-control">
                    <option value="true">Công khai</option><option value="false">Riêng tư</option>
                </select>
            </div>
        </div>
    `, [{ text: 'Tạo nhóm', cls: 'btn-primary', onclick: createGroup }]);
}

async function createGroup() {
    try {
        const body = {
            name: document.getElementById('g_name').value.trim(),
            subject: document.getElementById('g_subject').value.trim(),
            description: document.getElementById('g_desc').value.trim(),
            maxMembers: parseInt(document.getElementById('g_max').value, 10) || 50,
            isPublic: document.getElementById('g_public').value === 'true'
        };
        if (!body.name || !body.subject) return alert('Vui lòng nhập tên và môn học');
        await api('/groups', { method: 'POST', body: JSON.stringify(body) });
        closeModal(); loadGroups();
    } catch (e) { alert(e.message); }
}

async function editGroup(id) {
    try {
        const r = await api(`/groups/${id}`);
        const g = r.data;
        openModal('Sửa nhóm học', `
            <div class="form-group"><label>Tên nhóm</label><input id="g_name" class="form-control" value="${escapeHtml(g.name)}"></div>
            <div class="form-group"><label>Môn học</label><input id="g_subject" class="form-control" value="${escapeHtml(g.subject)}"></div>
            <div class="form-group"><label>Mô tả</label><textarea id="g_desc" class="form-control">${escapeHtml(g.description)}</textarea></div>
            <div class="form-row">
                <div class="form-group"><label>Số thành viên tối đa</label><input id="g_max" class="form-control" type="number" value="${g.maxMembers}"></div>
                <div class="form-group"><label>Hiển thị</label>
                    <select id="g_public" class="form-control">
                        <option value="true" ${g.isPublic ? 'selected' : ''}>Công khai</option>
                        <option value="false" ${!g.isPublic ? 'selected' : ''}>Riêng tư</option>
                    </select>
                </div>
            </div>
        `, [{ text: 'Lưu', cls: 'btn-primary', onclick: () => updateGroup(id) }]);
    } catch (e) { alert(e.message); }
}

async function updateGroup(id) {
    try {
        const body = {
            name: document.getElementById('g_name').value.trim(),
            subject: document.getElementById('g_subject').value.trim(),
            description: document.getElementById('g_desc').value.trim(),
            maxMembers: parseInt(document.getElementById('g_max').value, 10),
            isPublic: document.getElementById('g_public').value === 'true'
        };
        await api(`/groups/${id}`, { method: 'PUT', body: JSON.stringify(body) });
        closeModal(); loadGroups();
    } catch (e) { alert(e.message); }
}

async function deleteGroup(id) {
    if (!confirm('Xoá nhóm này? Toàn bộ tài liệu, lịch, tin nhắn sẽ bị xoá.')) return;
    try { await api(`/groups/${id}`, { method: 'DELETE' }); loadGroups(); }
    catch (e) { alert(e.message); }
}

async function leaveGroup(id) {
    if (!confirm('Rời nhóm này?')) return;
    try { await api(`/groups/${id}/leave`, { method: 'POST' }); loadGroups(); }
    catch (e) { alert(e.message); }
}

function showJoinGroupModal() {
    openModal('Tham gia nhóm bằng mã mời', `
        <div class="form-group"><label>Mã mời</label><input id="join_code" class="form-control" placeholder="Nhập mã mời"></div>
    `, [{ text: 'Tham gia', cls: 'btn-primary', onclick: joinGroup }]);
}

async function joinGroup() {
    try {
        const inviteCode = document.getElementById('join_code').value.trim().toUpperCase();
        if (!inviteCode) return alert('Vui lòng nhập mã mời');
        await api('/groups/join', { method: 'POST', body: JSON.stringify({ inviteCode }) });
        closeModal(); loadGroups();
    } catch (e) { alert(e.message); }
}

// ---------- Documents ----------
async function loadDocumentGroupsAndList() {
    await ensureMyGroups();
    const sel = document.getElementById('docGroupFilter');
    sel.innerHTML = '<option value="">-- Chọn nhóm --</option>' +
        myGroupsCache.map(g => `<option value="${g.id}">${escapeHtml(g.name)}</option>`).join('');
    if (myGroupsCache.length > 0 && !sel.value) sel.value = myGroupsCache[0].id;
    loadDocuments();
}

async function loadDocuments() {
    const groupId = document.getElementById('docGroupFilter').value;
    if (!groupId) {
        document.getElementById('documentsTable').innerHTML = `<div class="empty-state"><i class="fas fa-folder-open"></i><p>Chọn nhóm để xem tài liệu</p></div>`;
        return;
    }
    try {
        const r = await api(`/documents/by-group/${groupId}`);
        const rows = r.data.map(d => [
            `<strong>${escapeHtml(d.title)}</strong><div style="font-size:0.85rem;color:var(--text-muted)">${escapeHtml(d.fileName)}</div>`,
            escapeHtml(d.uploadedByName),
            fmtBytes(d.fileSize),
            d.downloadCount,
            fmtDate(d.createdAt),
            `<a class="btn btn-sm btn-outline" href="${API}/documents/${d.id}/download" target="_blank"><i class="fas fa-download"></i></a>
             <button class="btn btn-sm btn-warning" onclick="editDoc(${d.id})"><i class="fas fa-edit"></i></button>
             <button class="btn btn-sm btn-danger" onclick="deleteDoc(${d.id})"><i class="fas fa-trash"></i></button>`
        ]);
        document.getElementById('documentsTable').innerHTML = renderTable(
            ['Tài liệu', 'Người tải lên', 'Kích thước', 'Tải xuống', 'Ngày', 'Thao tác'], rows);
    } catch (e) {
        document.getElementById('documentsTable').innerHTML = `<div class="alert alert-danger">${e.message}</div>`;
    }
}

function showUploadDocModal() {
    if (myGroupsCache.length === 0) return alert('Bạn chưa tham gia nhóm nào.');
    openModal('Tải tài liệu lên', `
        <div class="form-group"><label>Nhóm *</label>
            <select id="d_group" class="form-control">
                ${myGroupsCache.map(g => `<option value="${g.id}">${escapeHtml(g.name)}</option>`).join('')}
            </select>
        </div>
        <div class="form-group"><label>Tiêu đề *</label><input id="d_title" class="form-control"></div>
        <div class="form-group"><label>Mô tả</label><textarea id="d_desc" class="form-control"></textarea></div>
        <div class="form-group"><label>Tags (cách nhau bằng dấu phẩy)</label><input id="d_tags" class="form-control"></div>
        <div class="form-group"><label>Tệp *</label><input id="d_file" class="form-control" type="file"></div>
    `, [{ text: 'Tải lên', cls: 'btn-primary', onclick: uploadDoc }]);
    const cur = document.getElementById('docGroupFilter').value;
    if (cur) document.getElementById('d_group').value = cur;
}

async function uploadDoc() {
    try {
        const file = document.getElementById('d_file').files[0];
        if (!file) return alert('Vui lòng chọn tệp');
        const fd = new FormData();
        fd.append('studyGroupId', document.getElementById('d_group').value);
        fd.append('title', document.getElementById('d_title').value.trim());
        fd.append('description', document.getElementById('d_desc').value.trim());
        fd.append('tags', document.getElementById('d_tags').value.trim());
        fd.append('file', file);
        await api('/documents/upload', { method: 'POST', body: fd });
        closeModal(); loadDocuments();
    } catch (e) { alert(e.message); }
}

async function editDoc(id) {
    try {
        const r = await api(`/documents/${id}`);
        const d = r.data;
        openModal('Sửa tài liệu', `
            <div class="form-group"><label>Tiêu đề</label><input id="d_title" class="form-control" value="${escapeHtml(d.title)}"></div>
            <div class="form-group"><label>Mô tả</label><textarea id="d_desc" class="form-control">${escapeHtml(d.description || '')}</textarea></div>
            <div class="form-group"><label>Tags</label><input id="d_tags" class="form-control" value="${escapeHtml(d.tags || '')}"></div>
        `, [{ text: 'Lưu', cls: 'btn-primary', onclick: () => updateDoc(id) }]);
    } catch (e) { alert(e.message); }
}

async function updateDoc(id) {
    try {
        await api(`/documents/${id}`, { method: 'PUT', body: JSON.stringify({
            title: document.getElementById('d_title').value.trim(),
            description: document.getElementById('d_desc').value.trim(),
            tags: document.getElementById('d_tags').value.trim()
        })});
        closeModal(); loadDocuments();
    } catch (e) { alert(e.message); }
}

async function deleteDoc(id) {
    if (!confirm('Xoá tài liệu này?')) return;
    try { await api(`/documents/${id}`, { method: 'DELETE' }); loadDocuments(); }
    catch (e) { alert(e.message); }
}

// ---------- Schedules ----------
async function loadScheduleGroupsAndList() {
    await ensureMyGroups();
    const sel = document.getElementById('scheduleGroupFilter');
    sel.innerHTML = '<option value="">-- Lịch sắp tới của tôi --</option>' +
        myGroupsCache.map(g => `<option value="${g.id}">${escapeHtml(g.name)}</option>`).join('');
    loadSchedules();
}

async function loadSchedules() {
    const groupId = document.getElementById('scheduleGroupFilter').value;
    try {
        const r = groupId ? await api(`/schedules/by-group/${groupId}`) : await api('/schedules/upcoming?days=30');
        const rows = r.data.map(s => [
            escapeHtml(s.title),
            escapeHtml(s.groupName),
            fmtDate(s.startTime),
            fmtDate(s.endTime),
            escapeHtml(s.location || '') || (s.meetingUrl ? `<a href="${escapeHtml(s.meetingUrl)}" target="_blank">Online</a>` : ''),
            `<span class="badge ${s.status === 'Upcoming' ? 'info' : s.status === 'Completed' ? 'success' : s.status === 'Cancelled' ? 'danger' : 'warning'}">${s.status}</span>`,
            `<button class="btn btn-sm btn-warning" onclick="editSchedule(${s.id})"><i class="fas fa-edit"></i></button>
             <button class="btn btn-sm btn-danger" onclick="deleteSchedule(${s.id})"><i class="fas fa-trash"></i></button>`
        ]);
        document.getElementById('schedulesTable').innerHTML = renderTable(
            ['Tiêu đề', 'Nhóm', 'Bắt đầu', 'Kết thúc', 'Địa điểm', 'Trạng thái', 'Thao tác'], rows);
    } catch (e) {
        document.getElementById('schedulesTable').innerHTML = `<div class="alert alert-danger">${e.message}</div>`;
    }
}

function showCreateScheduleModal() {
    if (myGroupsCache.length === 0) return alert('Bạn chưa tham gia nhóm nào.');
    const tomorrow = new Date(Date.now() + 86400000).toISOString().slice(0, 16);
    const after = new Date(Date.now() + 86400000 + 7200000).toISOString().slice(0, 16);
    openModal('Tạo lịch học', `
        <div class="form-group"><label>Nhóm *</label>
            <select id="s_group" class="form-control">
                ${myGroupsCache.map(g => `<option value="${g.id}">${escapeHtml(g.name)}</option>`).join('')}
            </select>
        </div>
        <div class="form-group"><label>Tiêu đề *</label><input id="s_title" class="form-control"></div>
        <div class="form-group"><label>Mô tả</label><textarea id="s_desc" class="form-control"></textarea></div>
        <div class="form-row">
            <div class="form-group"><label>Bắt đầu *</label><input id="s_start" class="form-control" type="datetime-local" value="${tomorrow}"></div>
            <div class="form-group"><label>Kết thúc *</label><input id="s_end" class="form-control" type="datetime-local" value="${after}"></div>
        </div>
        <div class="form-row">
            <div class="form-group"><label>Địa điểm</label><input id="s_loc" class="form-control" placeholder="Phòng B201"></div>
            <div class="form-group"><label>Link online</label><input id="s_url" class="form-control" placeholder="https://..."></div>
        </div>
    `, [{ text: 'Tạo lịch', cls: 'btn-primary', onclick: createSchedule }]);
}

async function createSchedule() {
    try {
        await api('/schedules', { method: 'POST', body: JSON.stringify({
            studyGroupId: parseInt(document.getElementById('s_group').value, 10),
            title: document.getElementById('s_title').value.trim(),
            description: document.getElementById('s_desc').value.trim(),
            startTime: new Date(document.getElementById('s_start').value).toISOString(),
            endTime: new Date(document.getElementById('s_end').value).toISOString(),
            location: document.getElementById('s_loc').value.trim(),
            meetingUrl: document.getElementById('s_url').value.trim()
        })});
        closeModal(); loadSchedules();
    } catch (e) { alert(e.message); }
}

async function editSchedule(id) {
    try {
        const r = await api(`/schedules/${id}`);
        const s = r.data;
        openModal('Sửa lịch học', `
            <div class="form-group"><label>Tiêu đề</label><input id="s_title" class="form-control" value="${escapeHtml(s.title)}"></div>
            <div class="form-group"><label>Mô tả</label><textarea id="s_desc" class="form-control">${escapeHtml(s.description || '')}</textarea></div>
            <div class="form-row">
                <div class="form-group"><label>Bắt đầu</label><input id="s_start" class="form-control" type="datetime-local" value="${new Date(s.startTime).toISOString().slice(0,16)}"></div>
                <div class="form-group"><label>Kết thúc</label><input id="s_end" class="form-control" type="datetime-local" value="${new Date(s.endTime).toISOString().slice(0,16)}"></div>
            </div>
            <div class="form-row">
                <div class="form-group"><label>Địa điểm</label><input id="s_loc" class="form-control" value="${escapeHtml(s.location || '')}"></div>
                <div class="form-group"><label>Link online</label><input id="s_url" class="form-control" value="${escapeHtml(s.meetingUrl || '')}"></div>
            </div>
            <div class="form-group"><label>Trạng thái</label>
                <select id="s_status" class="form-control">
                    ${['Upcoming','Ongoing','Completed','Cancelled'].map(x => `<option value="${x}" ${s.status === x ? 'selected' : ''}>${x}</option>`).join('')}
                </select>
            </div>
        `, [{ text: 'Lưu', cls: 'btn-primary', onclick: () => updateSchedule(id) }]);
    } catch (e) { alert(e.message); }
}

async function updateSchedule(id) {
    try {
        await api(`/schedules/${id}`, { method: 'PUT', body: JSON.stringify({
            title: document.getElementById('s_title').value.trim(),
            description: document.getElementById('s_desc').value.trim(),
            startTime: new Date(document.getElementById('s_start').value).toISOString(),
            endTime: new Date(document.getElementById('s_end').value).toISOString(),
            location: document.getElementById('s_loc').value.trim(),
            meetingUrl: document.getElementById('s_url').value.trim(),
            status: document.getElementById('s_status').value
        })});
        closeModal(); loadSchedules();
    } catch (e) { alert(e.message); }
}

async function deleteSchedule(id) {
    if (!confirm('Xoá lịch này?')) return;
    try { await api(`/schedules/${id}`, { method: 'DELETE' }); loadSchedules(); }
    catch (e) { alert(e.message); }
}

// ---------- Progress ----------
async function loadProgressGroupsAndList() {
    await ensureMyGroups();
    const sel = document.getElementById('progressGroupFilter');
    sel.innerHTML = '<option value="">-- Tiến độ của tôi --</option>' +
        myGroupsCache.map(g => `<option value="${g.id}">Nhóm: ${escapeHtml(g.name)}</option>`).join('');
    loadProgress();
}

async function loadProgress() {
    const groupId = document.getElementById('progressGroupFilter').value;
    try {
        const r = groupId ? await api(`/progress/by-group/${groupId}`) : await api('/progress/mine');
        const rows = r.data.map(p => [
            escapeHtml(p.userName),
            escapeHtml(p.title),
            escapeHtml(p.groupName),
            `<div class="progress-bar"><div class="progress-fill" style="width:${p.completionPercent}%"></div></div>
             <small>${p.completionPercent}%</small>`,
            `<span class="badge ${p.status === 'Completed' ? 'success' : p.status === 'InProgress' ? 'warning' : 'secondary'}">${p.status}</span>`,
            fmtDate(p.updatedAt || p.createdAt),
            (p.userId === currentUser.userId || currentUser.role === 'Admin')
                ? `<button class="btn btn-sm btn-warning" onclick="editProgress(${p.id})"><i class="fas fa-edit"></i></button>
                   <button class="btn btn-sm btn-danger" onclick="deleteProgress(${p.id})"><i class="fas fa-trash"></i></button>`
                : ''
        ]);
        document.getElementById('progressTable').innerHTML = renderTable(
            ['Người học', 'Mục tiêu', 'Nhóm', 'Tiến độ', 'Trạng thái', 'Cập nhật', 'Thao tác'], rows);
    } catch (e) {
        document.getElementById('progressTable').innerHTML = `<div class="alert alert-danger">${e.message}</div>`;
    }
}

function showCreateProgressModal() {
    if (myGroupsCache.length === 0) return alert('Bạn chưa tham gia nhóm nào.');
    openModal('Thêm mục tiêu học tập', `
        <div class="form-group"><label>Nhóm *</label>
            <select id="p_group" class="form-control">
                ${myGroupsCache.map(g => `<option value="${g.id}">${escapeHtml(g.name)}</option>`).join('')}
            </select>
        </div>
        <div class="form-group"><label>Tiêu đề *</label><input id="p_title" class="form-control"></div>
        <div class="form-group"><label>Mô tả</label><textarea id="p_desc" class="form-control"></textarea></div>
        <div class="form-row">
            <div class="form-group"><label>Trạng thái</label>
                <select id="p_status" class="form-control">
                    <option>NotStarted</option><option>InProgress</option><option>Completed</option>
                </select>
            </div>
            <div class="form-group"><label>% hoàn thành</label><input id="p_pct" class="form-control" type="number" min="0" max="100" value="0"></div>
        </div>
    `, [{ text: 'Tạo', cls: 'btn-primary', onclick: createProgress }]);
}

async function createProgress() {
    try {
        await api('/progress', { method: 'POST', body: JSON.stringify({
            studyGroupId: parseInt(document.getElementById('p_group').value, 10),
            title: document.getElementById('p_title').value.trim(),
            description: document.getElementById('p_desc').value.trim(),
            status: document.getElementById('p_status').value,
            completionPercent: parseInt(document.getElementById('p_pct').value, 10) || 0
        })});
        closeModal(); loadProgress();
    } catch (e) { alert(e.message); }
}

async function editProgress(id) {
    try {
        const r = await api(`/progress/${id}`);
        const p = r.data;
        openModal('Cập nhật tiến độ', `
            <div class="form-group"><label>Tiêu đề</label><input id="p_title" class="form-control" value="${escapeHtml(p.title)}"></div>
            <div class="form-group"><label>Mô tả</label><textarea id="p_desc" class="form-control">${escapeHtml(p.description || '')}</textarea></div>
            <div class="form-row">
                <div class="form-group"><label>Trạng thái</label>
                    <select id="p_status" class="form-control">
                        ${['NotStarted','InProgress','Completed'].map(x => `<option value="${x}" ${p.status === x ? 'selected' : ''}>${x}</option>`).join('')}
                    </select>
                </div>
                <div class="form-group"><label>% hoàn thành</label><input id="p_pct" class="form-control" type="number" min="0" max="100" value="${p.completionPercent}"></div>
            </div>
        `, [{ text: 'Lưu', cls: 'btn-primary', onclick: () => updateProgress(id) }]);
    } catch (e) { alert(e.message); }
}

async function updateProgress(id) {
    try {
        await api(`/progress/${id}`, { method: 'PUT', body: JSON.stringify({
            title: document.getElementById('p_title').value.trim(),
            description: document.getElementById('p_desc').value.trim(),
            status: document.getElementById('p_status').value,
            completionPercent: parseInt(document.getElementById('p_pct').value, 10) || 0
        })});
        closeModal(); loadProgress();
    } catch (e) { alert(e.message); }
}

async function deleteProgress(id) {
    if (!confirm('Xoá mục tiêu này?')) return;
    try { await api(`/progress/${id}`, { method: 'DELETE' }); loadProgress(); }
    catch (e) { alert(e.message); }
}

// ---------- Chat ----------
let chatPollTimer = null;

async function loadChatGroups() {
    await ensureMyGroups();
    const sel = document.getElementById('chatGroupSelect');
    sel.innerHTML = '<option value="">-- Chọn nhóm --</option>' +
        myGroupsCache.map(g => `<option value="${g.id}">${escapeHtml(g.name)}</option>`).join('');
    if (myGroupsCache.length > 0 && !sel.value) sel.value = myGroupsCache[0].id;
    loadChat();
}

async function loadChat() {
    if (chatPollTimer) clearInterval(chatPollTimer);
    const groupId = document.getElementById('chatGroupSelect').value;
    if (!groupId) {
        document.getElementById('chatList').innerHTML = '<div class="empty-state"><i class="fas fa-comments"></i><p>Chọn một nhóm để bắt đầu trò chuyện</p></div>';
        return;
    }
    await renderChat(groupId);
    chatPollTimer = setInterval(() => renderChat(groupId), 5000);
}

async function renderChat(groupId) {
    try {
        const r = await api(`/chat/by-group/${groupId}?take=50`);
        const list = document.getElementById('chatList');
        list.innerHTML = r.data.map(m => `
            <div class="chat-msg ${m.senderId === currentUser.userId ? 'mine' : ''}">
                <div class="meta"><strong>${escapeHtml(m.senderName)}</strong> · ${fmtDate(m.sentAt)}</div>
                <div>${escapeHtml(m.content)}</div>
            </div>`).join('');
        list.scrollTop = list.scrollHeight;
    } catch (e) {
        document.getElementById('chatList').innerHTML = `<div class="alert alert-danger">${e.message}</div>`;
    }
}

async function sendChat() {
    const groupId = document.getElementById('chatGroupSelect').value;
    const input = document.getElementById('chatInput');
    const content = input.value.trim();
    if (!groupId || !content) return;
    try {
        await api('/chat/send', { method: 'POST', body: JSON.stringify({ studyGroupId: parseInt(groupId, 10), content }) });
        input.value = '';
        renderChat(groupId);
    } catch (e) { alert(e.message); }
}

// ---------- Notifications ----------
async function loadNotifications() {
    try {
        const r = await api('/notifications?take=100');
        const list = r.data;
        if (list.length === 0) {
            document.getElementById('notificationsList').innerHTML = '<div class="empty-state"><i class="fas fa-bell-slash"></i><p>Bạn chưa có thông báo</p></div>';
        } else {
            document.getElementById('notificationsList').innerHTML = list.map(n => `
                <div class="card" style="margin-bottom:0.5rem;${n.isRead ? 'opacity:0.7' : 'border-left:3px solid var(--primary)'}">
                    <div style="display:flex;justify-content:space-between;gap:1rem">
                        <div>
                            <strong>${escapeHtml(n.title)}</strong>
                            <span class="badge primary" style="margin-left:0.5rem">${n.type}</span>
                            <p style="margin-top:0.4rem;color:var(--text-muted)">${escapeHtml(n.message)}</p>
                            <small style="color:var(--text-muted)">${fmtDate(n.createdAt)}</small>
                        </div>
                        <div style="display:flex;gap:0.4rem;align-items:flex-start">
                            ${!n.isRead ? `<button class="btn btn-sm btn-outline" onclick="markRead(${n.id})"><i class="fas fa-check"></i></button>` : ''}
                            <button class="btn btn-sm btn-danger" onclick="deleteNotification(${n.id})"><i class="fas fa-trash"></i></button>
                        </div>
                    </div>
                </div>`).join('');
        }
        refreshUnreadBadge();
    } catch (e) {
        document.getElementById('notificationsList').innerHTML = `<div class="alert alert-danger">${e.message}</div>`;
    }
}

async function markRead(id) { try { await api(`/notifications/${id}/read`, { method: 'PUT' }); loadNotifications(); } catch (e) { alert(e.message); } }
async function markAllRead() { try { await api('/notifications/read-all', { method: 'PUT' }); loadNotifications(); } catch (e) { alert(e.message); } }
async function deleteNotification(id) { try { await api(`/notifications/${id}`, { method: 'DELETE' }); loadNotifications(); } catch (e) { alert(e.message); } }

async function refreshUnreadBadge() {
    if (!token) return;
    try {
        const r = await api('/notifications/count-unread');
        const badge = document.getElementById('navUnread');
        if (r.data > 0) { badge.textContent = r.data; badge.style.display = ''; }
        else badge.style.display = 'none';
    } catch { /* ignore */ }
}

// ---------- Users (admin) ----------
async function loadUsers() {
    if (currentUser?.role !== 'Admin') return;
    try {
        const search = document.getElementById('userSearch')?.value?.trim() || '';
        const r = await api(`/users?search=${encodeURIComponent(search)}`);
        const rows = r.data.map(u => [
            `<strong>${escapeHtml(u.fullName)}</strong>`,
            escapeHtml(u.email),
            escapeHtml(u.phone),
            `<span class="badge ${u.role === 'Admin' ? 'danger' : u.role === 'Leader' ? 'primary' : 'secondary'}">${u.role}</span>`,
            u.isActive ? '<span class="badge success">Hoạt động</span>' : '<span class="badge danger">Đã khoá</span>',
            fmtDate(u.createdAt),
            `<select onchange="changeUserRole(${u.id}, this.value)" class="form-control" style="width:auto;display:inline-block">
                ${['Member','Leader','Admin'].map(r => `<option value="${r}" ${u.role === r ? 'selected' : ''}>${r}</option>`).join('')}
             </select>
             <button class="btn btn-sm ${u.isActive ? 'btn-danger' : 'btn-success'}" onclick="toggleUserActive(${u.id}, ${!u.isActive})">
                ${u.isActive ? '<i class="fas fa-lock"></i>' : '<i class="fas fa-unlock"></i>'}
             </button>`
        ]);
        document.getElementById('usersTable').innerHTML = renderTable(
            ['Tên', 'Email', 'SĐT', 'Vai trò', 'Trạng thái', 'Ngày tạo', 'Thao tác'], rows);
    } catch (e) {
        document.getElementById('usersTable').innerHTML = `<div class="alert alert-danger">${e.message}</div>`;
    }
}

async function changeUserRole(id, role) {
    try { await api(`/users/${id}/role`, { method: 'PUT', body: JSON.stringify({ role }) }); loadUsers(); }
    catch (e) { alert(e.message); }
}

async function toggleUserActive(id, isActive) {
    try { await api(`/users/${id}/active`, { method: 'PUT', body: JSON.stringify({ isActive }) }); loadUsers(); }
    catch (e) { alert(e.message); }
}

// ---------- Modal ----------
function openModal(title, bodyHtml, buttons) {
    closeModal();
    const html = `
        <div class="modal-overlay" onclick="if(event.target===this)closeModal()">
            <div class="modal">
                <div class="modal-header">
                    <h3>${escapeHtml(title)}</h3>
                    <button class="modal-close" onclick="closeModal()"><i class="fas fa-times"></i></button>
                </div>
                <div class="modal-body">${bodyHtml}</div>
                <div class="modal-footer">
                    <button class="btn btn-outline" onclick="closeModal()">Đóng</button>
                    ${(buttons || []).map((b, i) => `<button class="btn ${b.cls}" data-btn-idx="${i}">${escapeHtml(b.text)}</button>`).join('')}
                </div>
            </div>
        </div>`;
    document.getElementById('modalRoot').innerHTML = html;
    (buttons || []).forEach((b, i) => {
        document.querySelector(`[data-btn-idx="${i}"]`)?.addEventListener('click', b.onclick);
    });
}

function closeModal() { document.getElementById('modalRoot').innerHTML = ''; }

// ---------- Helpers ----------
async function ensureMyGroups() {
    try {
        const r = await api('/groups/mine');
        myGroupsCache = r.data;
    } catch { myGroupsCache = []; }
}

// ---------- Boot ----------
window.addEventListener('DOMContentLoaded', () => {
    if (token && currentUser) showApp();
    setInterval(refreshUnreadBadge, 30000);
});
