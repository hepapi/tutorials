const express = require('express');
const bodyParser = require('body-parser');
const path = require('path');
const fs = require('fs');
const app = express();
const port = 3000;

const dataFilePath = path.join(__dirname, 'data', 'data.json');

let data = [];
if (fs.existsSync(dataFilePath)) {
  data = JSON.parse(fs.readFileSync(dataFilePath, 'utf8'));
}

app.use(bodyParser.json());
app.use(bodyParser.urlencoded({ extended: true }));

app.get('/', (req, res) => {
  res.sendFile(path.join(__dirname, 'index.html'));
});

app.post('/data', (req, res) => {
  data.push(req.body);
  fs.writeFileSync(dataFilePath, JSON.stringify(data, null, 2));
  res.redirect('/');
});

app.get('/data', (req, res) => {
  res.json(data);
});

app.listen(port, () => {
  console.log(`Example app listening at http://localhost:${port}`);
});
