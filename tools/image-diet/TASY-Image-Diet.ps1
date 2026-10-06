Add-Type -AssemblyName System.Windows.Forms
Add-Type -AssemblyName System.Drawing

$AppName = "TASY Image Diet"
$SiteUrl = "https://tasy.pro/"
$MaxSide = 1400
$JpegQ = 4

function Find-FFmpeg {
    $cmd = Get-Command ffmpeg.exe -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }
    $local = Join-Path $PSScriptRoot "ffmpeg.exe"
    if (Test-Path $local) { return $local }
    return $null
}

$ffmpeg = Find-FFmpeg
$files = New-Object System.Collections.ArrayList

$form = New-Object System.Windows.Forms.Form
$form.Text = $AppName
$form.Size = New-Object System.Drawing.Size(720,600)
$form.MinimumSize = New-Object System.Drawing.Size(620,520)
$form.StartPosition = "CenterScreen"
$form.AllowDrop = $true
$form.Font = New-Object System.Drawing.Font("Segoe UI",10)
$form.BackColor = [System.Drawing.Color]::White

$title = New-Object System.Windows.Forms.Label
$title.Text = "TASY Image Diet"
$title.Font = New-Object System.Drawing.Font("Segoe UI Semibold",22)
$title.AutoSize = $true
$title.Location = New-Object System.Drawing.Point(24,18)
$form.Controls.Add($title)

$sub = New-Object System.Windows.Forms.Label
$sub.Text = "Уменьшает вес изображений и удаляет метаданные."
$sub.AutoSize = $true
$sub.Location = New-Object System.Drawing.Point(27,62)
$form.Controls.Add($sub)

$spec = New-Object System.Windows.Forms.Label
$spec.Text = "JPEG · до 1400 px · без лишних данных"
$spec.ForeColor = [System.Drawing.Color]::DimGray
$spec.AutoSize = $true
$spec.Location = New-Object System.Drawing.Point(27,88)
$form.Controls.Add($spec)

$drop = New-Object System.Windows.Forms.Panel
$drop.Location = New-Object System.Drawing.Point(28,125)
$drop.Size = New-Object System.Drawing.Size(646,105)
$drop.Anchor = "Top,Left,Right"
$drop.BorderStyle = "FixedSingle"
$drop.AllowDrop = $true
$form.Controls.Add($drop)

$dropText = New-Object System.Windows.Forms.Label
$dropText.Text = "Перетащите изображения сюда`r`nили нажмите «Добавить файлы»"
$dropText.TextAlign = "MiddleCenter"
$dropText.Dock = "Fill"
$dropText.Font = New-Object System.Drawing.Font("Segoe UI",12)
$dropText.AllowDrop = $true
$drop.Controls.Add($dropText)

$list = New-Object System.Windows.Forms.ListBox
$list.Location = New-Object System.Drawing.Point(28,245)
$list.Size = New-Object System.Drawing.Size(646,120)
$list.Anchor = "Top,Bottom,Left,Right"
$list.HorizontalScrollbar = $true
$form.Controls.Add($list)

$add = New-Object System.Windows.Forms.Button
$add.Text = "Добавить файлы"
$add.Location = New-Object System.Drawing.Point(28,378)
$add.Size = New-Object System.Drawing.Size(145,34)
$add.Anchor = "Bottom,Left"
$form.Controls.Add($add)

$clear = New-Object System.Windows.Forms.Button
$clear.Text = "Очистить список"
$clear.Location = New-Object System.Drawing.Point(181,378)
$clear.Size = New-Object System.Drawing.Size(145,34)
$clear.Anchor = "Bottom,Left"
$form.Controls.Add($clear)

$outLabel = New-Object System.Windows.Forms.Label
$outLabel.Text = "Сохранить в:"
$outLabel.Location = New-Object System.Drawing.Point(28,429)
$outLabel.AutoSize = $true
$outLabel.Anchor = "Bottom,Left"
$form.Controls.Add($outLabel)

$outBox = New-Object System.Windows.Forms.TextBox
$outBox.Location = New-Object System.Drawing.Point(120,425)
$outBox.Size = New-Object System.Drawing.Size(445,28)
$outBox.Anchor = "Bottom,Left,Right"
$form.Controls.Add($outBox)

$browse = New-Object System.Windows.Forms.Button
$browse.Text = "Выбрать"
$browse.Location = New-Object System.Drawing.Point(574,423)
$browse.Size = New-Object System.Drawing.Size(100,32)
$browse.Anchor = "Bottom,Right"
$form.Controls.Add($browse)

$delete = New-Object System.Windows.Forms.CheckBox
$delete.Text = "Удалить оригиналы после успешной обработки"
$delete.Location = New-Object System.Drawing.Point(28,466)
$delete.AutoSize = $true
$delete.Anchor = "Bottom,Left"
$form.Controls.Add($delete)

$go = New-Object System.Windows.Forms.Button
$go.Text = "ОБРАБОТАТЬ"
$go.Font = New-Object System.Drawing.Font("Segoe UI Semibold",11)
$go.Location = New-Object System.Drawing.Point(28,500)
$go.Size = New-Object System.Drawing.Size(190,40)
$go.Anchor = "Bottom,Left"
$form.Controls.Add($go)

$status = New-Object System.Windows.Forms.Label
$status.Text = if ($ffmpeg) { "Готово к работе." } else { "FFmpeg не найден." }
$status.Location = New-Object System.Drawing.Point(235,510)
$status.Size = New-Object System.Drawing.Size(275,24)
$status.Anchor = "Bottom,Left,Right"
$form.Controls.Add($status)

$link = New-Object System.Windows.Forms.LinkLabel
$link.Text = "TASY.PRO ★★★★★"
$link.Location = New-Object System.Drawing.Point(535,510)
$link.AutoSize = $true
$link.Anchor = "Bottom,Right"
$link.LinkColor = [System.Drawing.Color]::Black
$link.ActiveLinkColor = [System.Drawing.Color]::DimGray
$form.Controls.Add($link)

function Add-Images([string[]]$paths) {
    foreach ($p in $paths) {
        if (Test-Path $p -PathType Container) {
            Add-Images ((Get-ChildItem -LiteralPath $p -File | Where-Object {$_.Extension -match '^\.(png|jpe?g)$'}).FullName)
        } elseif ([IO.Path]::GetExtension($p) -match '^\.(png|jpe?g)$') {
            if (-not $files.Contains($p)) {
                [void]$files.Add($p)
                [void]$list.Items.Add($p)
            }
        }
    }
    if ($files.Count -gt 0 -and [string]::IsNullOrWhiteSpace($outBox.Text)) {
        $outBox.Text = [IO.Path]::GetDirectoryName($files[0])
    }
}

$dropHandler = {
    if ($_.Data.GetDataPresent([Windows.Forms.DataFormats]::FileDrop)) {
        Add-Images ([string[]]$_.Data.GetData([Windows.Forms.DataFormats]::FileDrop))
    }
}
$drop.Add_DragEnter({ if ($_.Data.GetDataPresent([Windows.Forms.DataFormats]::FileDrop)) {$_.Effect="Copy"} })
$dropText.Add_DragEnter({ if ($_.Data.GetDataPresent([Windows.Forms.DataFormats]::FileDrop)) {$_.Effect="Copy"} })
$form.Add_DragEnter({ if ($_.Data.GetDataPresent([Windows.Forms.DataFormats]::FileDrop)) {$_.Effect="Copy"} })
$drop.Add_DragDrop($dropHandler)
$dropText.Add_DragDrop($dropHandler)
$form.Add_DragDrop($dropHandler)

$add.Add_Click({
    $d = New-Object System.Windows.Forms.OpenFileDialog
    $d.Multiselect = $true
    $d.Filter = "Изображения|*.png;*.jpg;*.jpeg|Все файлы|*.*"
    if ($d.ShowDialog() -eq "OK") { Add-Images $d.FileNames }
})

$clear.Add_Click({
    $files.Clear()
    $list.Items.Clear()
    $status.Text = "Список очищен."
})

$browse.Add_Click({
    $d = New-Object System.Windows.Forms.FolderBrowserDialog
    if ($outBox.Text -and (Test-Path $outBox.Text)) { $d.SelectedPath = $outBox.Text }
    if ($d.ShowDialog() -eq "OK") { $outBox.Text = $d.SelectedPath }
})

$link.Add_LinkClicked({ Start-Process $SiteUrl })

$go.Add_Click({
    if (-not $ffmpeg) {
        [Windows.Forms.MessageBox]::Show("FFmpeg не найден. Положите ffmpeg.exe рядом с программой или добавьте FFmpeg в PATH.", $AppName)
        return
    }
    if ($files.Count -eq 0) {
        [Windows.Forms.MessageBox]::Show("Добавьте хотя бы одно изображение.", $AppName)
        return
    }
    $dest = $outBox.Text.Trim()
    if (-not $dest) {
        [Windows.Forms.MessageBox]::Show("Выберите папку назначения.", $AppName)
        return
    }
    New-Item -ItemType Directory -Force -Path $dest | Out-Null

    $ok = 0; $failed = 0
    [long]$before = 0; [long]$after = 0
    $go.Enabled = $false
    $form.UseWaitCursor = $true

    foreach ($src in @($files)) {
        try {
            $in = Get-Item -LiteralPath $src -ErrorAction Stop
            $before += $in.Length
            $base = [IO.Path]::GetFileNameWithoutExtension($in.Name)
            $dst = Join-Path $dest ($base + ".jpg")

            # Avoid destroying source when input is already the exact output path.
            $samePath = ([IO.Path]::GetFullPath($src) -eq [IO.Path]::GetFullPath($dst))
            if ($samePath) {
                $tmp = Join-Path $dest ("__tasy_tmp__" + [Guid]::NewGuid().ToString("N") + ".jpg")
            } else { $tmp = $dst }

            $vf = "scale='if(gte(iw,ih),min($MaxSide,iw),-2)':'if(gte(iw,ih),-2,min($MaxSide,ih))'"
            & $ffmpeg -loglevel error -y -i $src -map_metadata -1 -frames:v 1 -vf $vf -q:v $JpegQ $tmp

            if ($LASTEXITCODE -eq 0 -and (Test-Path $tmp)) {
                if ($samePath) { Move-Item -Force $tmp $dst }
                $after += (Get-Item -LiteralPath $dst).Length
                if ($delete.Checked -and -not $samePath -and (Test-Path $src)) {
                    Remove-Item -LiteralPath $src -Force
                }
                $ok++
            } else {
                if (Test-Path $tmp) { Remove-Item $tmp -Force -ErrorAction SilentlyContinue }
                $failed++
            }
        } catch { $failed++ }
        $status.Text = "Обработано: $ok · ошибок: $failed"
        [Windows.Forms.Application]::DoEvents()
    }

    $go.Enabled = $true
    $form.UseWaitCursor = $false
    $mb = [math]::Round($before / 1MB, 1)
    $ma = [math]::Round($after / 1MB, 1)
    $status.Text = "Готово: $ok файлов · $mb МБ → $ma МБ"
})

[void]$form.ShowDialog()
