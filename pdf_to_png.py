import sys
import os
import fitz
print(fitz.__file__)
from PyQt5.QtWidgets import (QApplication, QMainWindow, QLabel, QPushButton, QFileDialog, QVBoxLayout, QWidget,
                             QListWidget, QHBoxLayout, QMessageBox, QProgressBar)
from PyQt5.QtCore import Qt, QThread, pyqtSignal
from concurrent.futures import ThreadPoolExecutor, as_completed

class ConversionThread(QThread):
    progress_update = pyqtSignal(int)
    conversion_complete = pyqtSignal()

    def __init__(self, pdf_paths):
        super().__init__()
        self.pdf_paths = pdf_paths

    def run(self):
        total_pages = sum(len(fitz.open(pdf)) for pdf in self.pdf_paths)
        pages_converted = 0

        with ThreadPoolExecutor(max_workers=os.cpu_count()) as executor:
            futures = []
            for pdf_path in self.pdf_paths:
                future = executor.submit(self.convert_pdf, pdf_path)
                futures.append(future)

            for future in as_completed(futures):
                pages_converted += future.result()
                self.progress_update.emit(int(pages_converted / total_pages * 100))

        self.conversion_complete.emit()

    def convert_pdf(self, pdf_path):
        pdf_name = os.path.splitext(os.path.basename(pdf_path))[0]
        output_folder = os.path.join(os.path.dirname(pdf_path), pdf_name)
        os.makedirs(output_folder, exist_ok=True)

        pdf_document = fitz.open(pdf_path)
        pages_converted = 0

        for page_num in range(len(pdf_document)):
            page = pdf_document.load_page(page_num)
            pix = page.get_pixmap(matrix=fitz.Matrix(300/72, 300/72))  # 300 DPI
            output_file = os.path.join(output_folder, f'page_{page_num + 1}.png')
            pix.save(output_file)
            pages_converted += 1

        pdf_document.close()
        return pages_converted

class PDFConverterApp(QMainWindow):
    def __init__(self):
        super().__init__()
        
        self.setWindowTitle('DashVerify PDF to PNG')
        self.setGeometry(300, 300, 600, 400)
        
        self.central_widget = QWidget(self)
        self.setCentralWidget(self.central_widget)
        self.layout = QVBoxLayout(self.central_widget)
        
        self.label = QLabel('Drag and Drop PDFs or Select Files', self)
        self.label.setAlignment(Qt.AlignCenter)
        self.label.setStyleSheet('QLabel {border: 2px dashed #aaa; padding: 20px;}')
        self.layout.addWidget(self.label)
        
        self.pdf_list = QListWidget(self)
        self.layout.addWidget(self.pdf_list)
        
        button_layout = QHBoxLayout()
        
        self.select_button = QPushButton('Select PDFs', self)
        self.select_button.clicked.connect(self.open_file_dialog)
        button_layout.addWidget(self.select_button)
        
        self.remove_button = QPushButton('Remove Selected', self)
        self.remove_button.clicked.connect(self.remove_selected_pdfs)
        button_layout.addWidget(self.remove_button)
        
        self.convert_button = QPushButton('Convert to PNG', self)
        self.convert_button.clicked.connect(self.convert_pdfs)
        button_layout.addWidget(self.convert_button)
        
        self.layout.addLayout(button_layout)
        
        self.progress_bar = QProgressBar(self)
        self.progress_bar.setVisible(False)
        self.layout.addWidget(self.progress_bar)
        
        self.setAcceptDrops(True)
        self.pdf_paths = []

    def dragEnterEvent(self, event):
        if event.mimeData().hasUrls():
            event.acceptProposedAction()
    
    def dropEvent(self, event):
        for url in event.mimeData().urls():
            file_path = url.toLocalFile()
            if file_path.endswith('.pdf') and file_path not in self.pdf_paths:
                self.pdf_paths.append(file_path)
                self.pdf_list.addItem(os.path.basename(file_path))
    
    def open_file_dialog(self):
        options = QFileDialog.Options()
        files, _ = QFileDialog.getOpenFileNames(self, 'Select PDF Files', '', 'PDF Files (*.pdf)', options=options)
        for file in files:
            if file not in self.pdf_paths:
                self.pdf_paths.append(file)
                self.pdf_list.addItem(os.path.basename(file))

    def remove_selected_pdfs(self):
        for item in self.pdf_list.selectedItems():
            index = self.pdf_list.row(item)
            self.pdf_list.takeItem(index)
            del self.pdf_paths[index]

    def convert_pdfs(self):
        if not self.pdf_paths:
            QMessageBox.warning(self, 'No PDFs', 'Please select at least one PDF file to convert.')
            return
        
        self.progress_bar.setVisible(True)
        self.progress_bar.setValue(0)
        self.convert_button.setEnabled(False)
        
        self.conversion_thread = ConversionThread(self.pdf_paths)
        self.conversion_thread.progress_update.connect(self.update_progress)
        self.conversion_thread.conversion_complete.connect(self.conversion_finished)
        self.conversion_thread.start()

    def update_progress(self, value):
        self.progress_bar.setValue(value)

    def conversion_finished(self):
        self.progress_bar.setVisible(False)
        self.convert_button.setEnabled(True)
        QMessageBox.information(self, 'Conversion Complete', 'All PDFs have been converted successfully.')

if __name__ == '__main__':
    app = QApplication(sys.argv)
    window = PDFConverterApp()
    window.show()
    sys.exit(app.exec_())
